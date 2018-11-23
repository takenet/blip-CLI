using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Take.ContentProvider.Domain.Contract.Model;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    public class InputWithTags
    {
        public string Input { get; set; }
        public List<Tag> Tags { get; set; }
        public string IntentExpected { get; set; }
        public List<string> EntitiesExpected { get; set; }
        public string AnswerExpected { get; set; }

        public static IEnumerable<Tag> GetTagsByString(string tags)
        {
            return tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t =>
                {
                    var tagkv = t.Split(':');
                    return new Tag
                    {
                        Key = tagkv[0],
                        Value = tagkv[1]
                    };
                });

        }

        public static string GetStringByTags(List<Tag> tags)
        {
            var stringList = tags.Select(t => t.Key + ':' + t.Value);
            return string.Join(',', stringList);

        }

        public static InputWithTags FromText(string text)
        {
            return new InputWithTags { Input = text };
        }

        public  static IEnumerable<InputWithTags> FromTextList(List<string> text)
        {
            return text.Select(t => FromText(t));
        }
    }
}
