﻿using System;

namespace AnimalCrossingFlowers
{
    public class InputException : Exception
    {
        public InputException(string message): base(message) { }
    }
}
