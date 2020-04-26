'use strict';

function geneSetToString(geneSet) {
    var string = '';
    for (var k = 0; k < geneSet.length; k++) {
        string += geneSet[k];
    }
    return string;
}

class BreedingPair {
    constructor(geneSet1, geneSet2) {
        this.geneSet1 = geneSet1;
        this.geneSet2 = geneSet2;
    }

    setChance(target) {
        this.chance = 1;
        for (var k = 0; k < target.length; k++) {
            var gene1 = this.geneSet1[k];
            var gene2 = this.geneSet2[k];
            if (gene1 == 0 && gene2 == 0) {
                if (target[k] != 0) {
                    return 0;
                }
            } else if (gene1 == 2 && gene2 == 2) {
                if (target[k] != 2) {
                    return 0;
                }
            } else if ((gene1 == 0 && gene2 == 2) || (gene1 == 2 && gene2 == 0)) {
                if (target[k] != 1) {
                    return 0;
                }
            } else if (gene1 + gene2 == 1) {
                if (target[k] == 2) {
                    return 0;
                } else {
                    this.chance /= 2;
                }
            } else if (gene1 + gene2 == 3) {
                if (target[k] == 0) {
                    return 0;
                } else {
                    this.chance /= 2;
                }
            } else if (gene1 == 1 && gene2 == 1) {
                if (target[k] == 1) {
                    this.chance /= 2;
                } else {
                    this.chance /= 4;
                }
            }
        }
    }

    toString() {
        var result = geneSetToString(this.geneSet1) + ' x ' + geneSetToString(this.geneSet2);
        if (this.chance) {
            result += ' (' + (this.chance * 100) + '%)';
        }
        return result;
    }
}

function cloneArray(array) {
    var clone = [];
    for (var k = 0; k < array.length; k++) {
        clone.push(array[k]);
    }
    return clone;
}

function getPairs(gene) {
    var list = [];
    if (gene == 0) {
        list.push([0, 0]);
    }
    if (gene == 0 || gene == 1) {
        list.push([0, 1]);
        list.push([1, 0]);
    }
    if (gene == 1) {
        list.push([0, 2]);
        list.push([2, 0]);
    }
    list.push([1, 1]);
    if (gene == 1 || gene == 2) {
        list.push([1, 2]);
        list.push([2, 1]);
    }
    if (gene == 2) {
        list.push([2, 2]);
    }
    return list;
}

function getFullPairs(geneSet) {
    var list = [new BreedingPair([], [])];
    for (var k = 0; k < geneSet.length; k++) {
        var genePairs = getPairs(geneSet[k]);
        var newList = [];
        for (var j = 0; j < list.length; j++) {
            var first = list[j].geneSet1;
            var second = list[j].geneSet2;
            for (var i = 0; i < genePairs.length; i++) {
                var newFirst = cloneArray(first);
                var newSecond = cloneArray(second);
                newFirst.push(genePairs[i][0]);
                newSecond.push(genePairs[i][1]);
                newList.push(new BreedingPair(newFirst, newSecond));
            }
        }
        list = newList;
    }
    list = exclude(list, geneSet);
    list.sort(comparePairs);
    for (var i = 0; i < list.length; i++) {
        for (var j = i + 1; j < list.length; j++) {
            if (geneSetsEqual(list[i].geneSet1, list[j].geneSet2) && geneSetsEqual(list[i].geneSet2, list[j].geneSet1)) {
                list.splice(j, 1);
                j--;
            }
        }
    }
    for (var k = 0; k < list.length; k++) {
        list[k].setChance(geneSet);
    }
    return list;
}

function comparePairs(pair1, pair2) {
    for (var k = 0; k < pair1.geneSet1.length; k++) {
        if (pair1.geneSet1[k] != pair2.geneSet1[k]) {
            return pair1.geneSet1[k] - pair2.geneSet1[k];
        }
    }
    for (var k = 0; k < pair1.geneSet2.length; k++) {
        if (pair1.geneSet2[k] != pair2.geneSet2[k]) {
            return pair1.geneSet2[k] - pair2.geneSet2[k];
        }
    }
    return 0;
}

function geneSetsEqual(geneSet1, geneSet2) {
    if (geneSet1.length != geneSet2.length) {
        return false;
    }
    for (var k = 0; k < geneSet1.length; k++) {
        if (geneSet1[k] != geneSet2[k]) {
            return false;
        }
    }
    return true;
}

function exclude(pairs, geneSet) {
    return pairs.filter(pair => !geneSetsEqual(pair.geneSet1, geneSet) && !geneSetsEqual(pair.geneSet2, geneSet));
}

function lockGene(pairs, index, value) {
    return pairs.filter(pair => pair.geneSet1[index] == value && pair.geneSet2[index] == value);
}

function filterByChance(pairs, chance) {
    return pairs.filter(pair => pair.chance == chance);
}

function printBreedingPairs(pairs) {
    console.log(JSON.stringify(pairs.map(pair => pair.toString()), null, 2));
}