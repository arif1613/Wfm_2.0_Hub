using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests
{
    public class OrderedDictionaryTests
    {
        private static OrderedDictionary<string, string> GetAlphabetDictionary(IEqualityComparer<string> comparer = null)
        {
            OrderedDictionary<string, string> alphabet = (comparer == null
                                                              ? new OrderedDictionary<string, string>()
                                                              : new OrderedDictionary<string, string>(comparer));
            for (var a = Convert.ToInt32('a'); a <= Convert.ToInt32('z'); a++)
            {
                var c = Convert.ToChar(a);
                alphabet.Add(c.ToString(), c.ToString().ToUpper());
            }
            //Assert.AreEqual(26, alphabet.Count);
            return alphabet;
        }

        private static List<KeyValuePair<string, string>> GetAlphabetList()
        {
            var alphabet = new List<KeyValuePair<string, string>>();
            for (var a = Convert.ToInt32('a'); a <= Convert.ToInt32('z'); a++)
            {
                var c = Convert.ToChar(a);
                alphabet.Add(new KeyValuePair<string, string>(c.ToString(), c.ToString().ToUpper()));
            }
            //Assert.AreEqual(26, alphabet.Count);
            return alphabet;
        }

        private It TestAdd = () =>
            {
                var od = new OrderedDictionary<string, string>();
                od.Count.ShouldEqual(0);
                od.IndexOf("foo").ShouldEqual(-1);

                od.Add("foo", "bar");
                od.Count.ShouldEqual(1);
                od.IndexOf("foo").ShouldEqual(0);
                od["foo"].ShouldEqual("bar");
                od["foo"].ShouldEqual("bar");
                od.GetItem(0).Key.ShouldEqual("foo");
                od.GetItem(0).Value.ShouldEqual("bar");
            };

        public It TestRemove = () =>
            {
                var od = new OrderedDictionary<string, string>();

                od.Add("foo", "bar");
                od.Count.ShouldEqual(1);

                od.Remove("foo");
                od.Count.ShouldEqual(0);
            };

        public It TestRemoveAt = () =>
            {
                var od = new OrderedDictionary<string, string>();

                od.Add("foo", "bar");
                od.Count.ShouldEqual(1);

                od.RemoveAt(0);
                od.Count.ShouldEqual(0);
            };

        public It TestClear = () =>
            {
                var od = GetAlphabetDictionary();
                od.Count.ShouldEqual(26);
                od.Clear();
                od.Count.ShouldEqual(0);
            };

        public It TestOrderIsPreserved = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                var alphabetList = GetAlphabetList();
                alphabetDict.Count.ShouldEqual(26);
                alphabetList.Count.ShouldEqual(26);

                var keys = alphabetDict.Keys.ToList();
                var values = alphabetDict.Values.ToList();

                for (var i = 0; i < 26; i++)
                {
                    var dictItem = alphabetDict.GetItem(i);
                    var listItem = alphabetList[i];
                    var key = keys[i];
                    var value = values[i];

                    dictItem.ShouldEqual(listItem);
                    key.ShouldEqual(listItem.Key);
                    value.ShouldEqual(listItem.Value);
                }
            };

        public It TestTryGetValue = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                string result = null;
                alphabetDict.TryGetValue("abc", out result).ShouldBeFalse();
                result.ShouldBeNull();
                alphabetDict.TryGetValue("z", out result).ShouldBeTrue();
                result.ShouldEqual("Z");
            };

        public It TestEnumerator = () =>
            {
                var alphabetDict = GetAlphabetDictionary();

                var keys = alphabetDict.Keys.ToList();
                keys.Count.ShouldEqual(26);

                var i = 0;
                foreach (var kvp in alphabetDict)
                {
                    var value = alphabetDict[kvp.Key];
                    kvp.Value.ShouldEqual(value);
                    i++;
                }
            };

        public It TestInvalidIndex = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                var ex = Catch.Exception(() =>
                    {
                        var notGonnaWork = alphabetDict[100];
                    });
                ex.Message.ShouldContain("index is outside the bounds");
            };

        public It TestMissingKey = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                var ex = Catch.Exception(() => { var notGonnaWork = alphabetDict["abc"]; });
                ex.Message.ShouldContain("key is not present");
            };

        public It TestUpdateExistingValue = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                alphabetDict.ContainsKey("c").ShouldBeTrue();
                alphabetDict.IndexOf("c").ShouldEqual(2);
                alphabetDict[2].ShouldEqual("C");
                alphabetDict[2] = "CCC";
                alphabetDict.ContainsKey("c").ShouldBeTrue();
                alphabetDict.IndexOf("c").ShouldEqual(2);
                alphabetDict[2].ShouldEqual("CCC");
            };

        public It TestInsertValue = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                alphabetDict.ContainsKey("c").ShouldBeTrue();
                alphabetDict.IndexOf("c").ShouldEqual(2);
                alphabetDict[2].ShouldEqual("C");
                alphabetDict.Count.ShouldEqual(26);
                alphabetDict.ContainsValue("ABC").ShouldBeFalse();

                alphabetDict.Insert(2, "abc", "ABC");
                alphabetDict.ContainsKey("c").ShouldBeTrue();
                alphabetDict.IndexOf("abc").ShouldEqual(2);
                alphabetDict[2].ShouldEqual("ABC");
                alphabetDict.Count.ShouldEqual(27);
                alphabetDict.ContainsValue("ABC").ShouldBeTrue();
            };

        public It TestValueComparer = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                alphabetDict.ContainsValue("a").ShouldBeFalse();
                alphabetDict.ContainsValue("a", StringComparer.OrdinalIgnoreCase).ShouldBeTrue();
            };

        public It TestSortByKeys = () =>
            {
                var alphabetDict = GetAlphabetDictionary();
                var reverseAlphabetDict = GetAlphabetDictionary();
                Comparison<string> stringReverse =
                    ((x, y) => (String.Equals(x, y) ? 0 : String.Compare(x, y) >= 1 ? -1 : 1));
                reverseAlphabetDict.SortKeys(stringReverse);
                for (int j = 0, k = 25; j < alphabetDict.Count; j++, k--)
                {
                    var ascValue = alphabetDict.GetItem(j);
                    var dscValue = reverseAlphabetDict.GetItem(k);
                    ascValue.Key.ShouldEqual(dscValue.Key);
                    ascValue.Value.ShouldEqual(dscValue.Value);
                }
            };
    }
}
