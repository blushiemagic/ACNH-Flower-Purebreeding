'use strict';

const fs = require('fs');

var raw = fs.readFileSync(process.argv[2], { encoding: 'utf8' });
var lines = raw.split('\n');
for (var k = 0; k < lines.length; k++) {
    lines[k] = lines[k].trim();
}

const numGenes = parseInt(lines[0]);
if (numGenes != 3 && numGenes != 4) {
    throw new Error("There must be 3 or 4 genes");
}
const numGeneSets = Math.pow(3, numGenes);
const colors = [];
const breedingPaths = [];
for (var k = 0; k < numGeneSets; k++) {
    colors.push(lines[k + 1]);
    breedingPaths.push(null);
}
const numBreedingPairs = numGeneSets * (numGeneSets - 1) / 2;
const breedingPairs = [];
for (var k = 0; k < numBreedingPairs; k++) {
    breedingPairs.push(null);
}

function getGene(uid, index) {
    return Math.floor(uid / Math.pow(3, numGenes - 1 - index)) % 3;
}

function getGeneSetUID(geneSet) {
    var result = 0;
    for (var k = 0; k < geneSet.length; k++) {
        result *= 3;
        result += geneSet[k];
    }
    return result;
}

function geneUIDToString(uid) {
    var result = '';
    for (var k = 0; k < numGenes; k++) {
        result += getGene(uid, k);
    }
    result += ' ' + colors[uid];
    return result;
}

function recombineGenes(gene1, gene2) {
    if (gene1 == 0 && gene2 == 0) {
        return [1, 0, 0];
    } else if (gene1 == 2 && gene2 == 2) {
        return [0, 0, 1];
    } else if (gene1 + gene2 == 1) { //0 x 1, 1 x 0
        return [0.5, 0.5, 0];
    } else if (gene1 + gene2 == 3) { //1 x 2, 2 x 1
        return [0, 0.5, 0.5];
    } else if (gene1 == 1 && gene2 == 1) {
        return [0.25, 0.5, 0.25];
    } else { //0 x 2, 2 x 0
        return [0, 1, 0];
    }
}

function recombineGeneSets(uid1, uid2) {
    var table = { 0: 1 };
    for (var k = 0; k < numGenes; k++) {
        var chances = recombineGenes(getGene(uid1, k), getGene(uid2, k));
        var newTable = {};
        for (var uidString in table) {
            var uid = parseInt(uidString);
            for (var j = 0; j < chances.length; j++) {
                if (chances[j] > 0) {
                    newTable[uid * 3 + j] = table[uid] * chances[j];
                }
            }
        }
        table = newTable;
    }
    return table;
}

class BreedingPair {
    constructor(uid1, uid2) {
        if (uid1 <= uid2) {
            this.uid1 = uid1;
            this.uid2 = uid2;
        } else {
            this.uid1 = uid2;
            this.uid2 = uid1;
        }
        this.results = recombineGeneSets(this.uid1, this.uid2);
    }

    getUID() {
        return getPairUID(this.uid1, this.uid2);
    }

    getChance(target) {
        return this.results[target];
    }

    getColorResults() {
        var table = {};
        for (var result in this.results) {
            var color = colors[result];
            if (!table[color]) {
                table[color] = 0;
            }
            table[color] += this.results[result];
        }
        return table;
    }
}

function getPairUID(geneSetUID1, geneSetUID2) {
    if (geneSetUID1 > geneSetUID2) {
        var temp = geneSetUID1;
        geneSetUID1 = geneSetUID2;
        geneSetUID2 = temp;
    }
    return numGeneSets * geneSetUID1 + geneSetUID2;
}

class BreedingPath {
    constructor(target, pair) {
        this.target = target;
        this.pair = pair;
        if (this.pair != null) {
            this.testAgainst = [];
            for (var resultString in pair.results) {
                var result = parseInt(resultString);
                if (result != this.target && colors[result] == colors[this.target]) {
                    this.testAgainst.push(result);
                }
            }
        }
        if (this.needsTest()) {
            this.test = null;
            this.testWith('self');
            for (var k = 0; k < processed.length; k++) {
                this.testWith(processed[k]);
            }
        }
    }

    getResistance() {
        if (this.pair == null) {
            return 0;
        }
        var resistance1 = breedingPaths[this.pair.uid1].getResistance();
        var resistance2 = breedingPaths[this.pair.uid2].getResistance();
        var result = Math.max(resistance1, resistance2);
        result += 1 / this.pair.getChance(this.target);
        if (this.needsTest()) {
            var colorChance = 0;
            for (var k = 0; k < this.testAgainst.length; k++) {
                colorChance += this.pair.getChance(this.testAgainst[k]);
            }
            var testMultiplier = colorChance / this.pair.getChance(this.target);
            result += this.testResistance * (1 + testMultiplier);
        }
        return result;
    }

    usesGeneSet(geneSetUID) {
        if (this.pair == null) {
            return false;
        }
        if (geneSetUID == this.pair.uid1 || geneSetUID == this.pair.uid2) {
            return true;
        }
        return breedingPaths[this.pair.uid1].usesGeneSet(geneSetUID)
            || breedingPaths[this.pair.uid2].usesGeneSet(geneSetUID);
    }

    needsTest() {
        return this.pair != null && this.testAgainst.length > 0;
    }

    testWith(test) {
        if (test == this.target || !breedingPaths[test] || breedingPaths[test].usesGeneSet(this.target)) {
            return;
        }
        var targetPairUID = getPairUID(this.target, test == 'self' ? this.target : test);
        var targetColors = breedingPairs[targetPairUID].getColorResults();
        for (var k = 0; k < this.testAgainst.length; k++) {
            var negativePairUID = getPairUID(this.testAgainst[k], test == 'self' ? this.testAgainst[k] : test);
            var negativeColors = breedingPairs[negativePairUID].getColorResults();
            for (var color in negativeColors) {
                delete targetColors[color];
            }
        }
        var chance = 0;
        for (var color in targetColors) {
            chance += targetColors[color];
        }
        var resistance = 1 / chance;
        if (this.test == null || resistance < this.testResistance) {
            this.test = test;
            this.testResistance = resistance;
        }
    }

    toString() {
        var target = geneUIDToString(this.target);
        var parent1 = geneUIDToString(this.pair.uid1);
        var parent2 = geneUIDToString(this.pair.uid2);
        var chance = 100 * this.pair.getChance(this.target);
        var result = target + ' = ' + parent1 + ' x ' + parent2 + ' (' + chance + '%)';
        if (this.test) {
            result += ' test: ' + (this.test == 'self' ? this.test : geneUIDToString(this.test));
            result += ' (' + (100 / this.testResistance) + '%)';
        }
        return result;
    }
}

var seeds = [];
for (var k = numGeneSets + 1; k < lines.length - 1; k++) {
    var geneSet = [parseInt(lines[k][0]), parseInt(lines[k][1]), parseInt(lines[k][2])];
    seeds.push(getGeneSetUID(geneSet));
}
for (var i = 0; i < numGeneSets; i++) {
    for (var j = i; j < numGeneSets; j++) {
        var pair = new BreedingPair(i, j);
        breedingPairs[pair.getUID()] = pair;
    }
}
var goals = lines[lines.length - 1].split(' ');
for (var k = 0; k < goals.length; k++) {
    goals[k] = goals[k].trim();
    var geneSet = [parseInt(goals[k][0]), parseInt(goals[k][1]), parseInt(goals[k][2])];
    goals[k] = getGeneSetUID(geneSet);
}

var processed = [];
var previous = [];
while (goals.length > 0) {
    var toProcess = [];
    for (var k = 0; k < seeds.length; k++) {
        breedingPaths[seeds[k]] = new BreedingPath(seeds[k], null);
        toProcess.push(seeds[k]);
    }
    for (var k = 0; k < previous.length; k++) {
        breedingPaths[previous[k]] = new BreedingPath(previous[k], null);
        toProcess.push(previous[k]);
    }
    while (toProcess.length > 0) {
        var processing = toProcess.shift();
        if (!processed.includes(processing)) {
            processed.push(processing);
        }
        for (var k = 0; k < processed.length; k++) {
            if (breedingPaths[processed[k]] && breedingPaths[processed[k]].needsTest()) {
                breedingPaths[processed[k]].testWith(processing);
            }
        }
        for (var k = 0; k < processed.length; k++) {
            var partner = processed[k];
            var pairUID = getPairUID(processing, partner);
            var pair = breedingPairs[pairUID];
            for (var targetString in pair.results) {
                var target = parseInt(targetString);
                var path = new BreedingPath(target, pair);
                var currentPath = breedingPaths[target];
                var isNew = currentPath == null;
                if (isNew || (path.getResistance() < currentPath.getResistance() && !path.usesGeneSet(target))) {
                    breedingPaths[target] = path;
                    if (!toProcess.includes(target)) {
                        toProcess.push(target);
                    }
                }
            }
        }
    }

    var easiestGoal = -1;
    var lowestResistance = Infinity;
    for (var k = 0; k < goals.length; k++) {
        if (breedingPaths[goals[k]]) {
            var resistance = breedingPaths[goals[k]].getResistance();
            if (resistance < lowestResistance) {
                easiestGoal = k;
                lowestResistance = resistance;
            }
        }
    }
    if (easiestGoal < 0) {
        break;
    }
    var steps = [];
    var stepsToProcess = [goals[easiestGoal]];
    while (stepsToProcess.length > 0) {
        var processing = stepsToProcess.shift();
        if (breedingPaths[processing].pair) {
            if (!steps.includes(processing)) {
                steps.unshift(processing);
            }
            stepsToProcess.push(breedingPaths[processing].pair.uid2);
            stepsToProcess.push(breedingPaths[processing].pair.uid1);
        }
    }
    console.log();
    for (var k = 0; k < steps.length; k++) {
        console.log(breedingPaths[steps[k]].toString());
    }

    processed = [];
    for (var k = 0; k < breedingPaths.length; k++) {
        breedingPaths[k] = null;
    }
    for (var k = 0; k < steps.length; k++) {
        previous.push(steps[k]);
    }
    goals.splice(easiestGoal, 1);
}