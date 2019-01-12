using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Domain.Interfaces;

namespace Infrastructure.Services
{
    public class InMemoryWordService : IWordService
    {
        private readonly Dictionary<string, string[]> _wordDict;

        public InMemoryWordService()
        {
            var path = Path.Combine(AppContext.BaseDirectory, @"Data\24k_lib.txt");
            var words = File.ReadAllLines(path);
            var wordHash = new HashSet<string>(words);
            _wordDict = wordHash
              .Select(word => new
              {
                  Key = string.Concat(word.OrderBy(c => c)),
                  Value = word
              })
              .GroupBy(item => item.Key, item => item.Value)
              .ToDictionary(chunk => chunk.Key, chunk => chunk.ToArray());
        }

        public IReadOnlyCollection<string> Find(string characters)
        {
            if (string.IsNullOrWhiteSpace(characters))
                return new List<string>();

            string source = string.Concat(characters.OrderBy(c => c)).ToLower();
            var words = Enumerable
              .Range(1, (1 << source.Length) - 1)
              .Select(index => string.Concat(source.Where((item, idx) => ((1 << idx) & index) != 0)))
              .SelectMany(key =>
              {
                  if (_wordDict.TryGetValue(key, out var foundWords))
                      return foundWords;
                  else
                      return new string[0];
              })
              .Distinct()
              .OrderByDescending(word => word.Length)
              .ToList();

            return words;
        }

        //https://stackoverflow.com/questions/39439197/find-words-in-wordlist-from-random-string-of-characters
    }
}
