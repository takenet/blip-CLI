﻿using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class AdaptedLevenshteinTextSimilarityService : ITextSimilarityService
    {
        private readonly IStringService _stringService;

        public AdaptedLevenshteinTextSimilarityService(IStringService stringService)
        {
            _stringService = stringService;
        }

        public double CalculateDistance(string text1, string text2)
        {
            return _stringService.LevenshteinDistance(text1, text2);
        }

        public double CalculateMinimumDistance(string text1, string text2)
        {
            int smallerStringSize = Math.Min(text1.Length, text1.Length);
            return Math.Max(1, 2 * Math.Log(smallerStringSize));
        }
    }
}