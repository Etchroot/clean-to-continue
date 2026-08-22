using System.Collections;
using CleanToContinue.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CleanToContinue.Tests.PlayMode
{
    public sealed class OpeningSequenceTests
    {
        [UnityTest]
        public IEnumerator SentencesAppearOneLineAtATimeWithoutRemovingEarlierLines()
        {
            var root = new GameObject("Opening Test");
            root.SetActive(false);
            var lineObject = new GameObject("Line", typeof(RectTransform), typeof(Text));
            lineObject.transform.SetParent(root.transform);
            var line = lineObject.GetComponent<Text>();
            var skipObject = new GameObject("Skip", typeof(RectTransform), typeof(Image), typeof(Button));
            skipObject.transform.SetParent(root.transform);
            var sequence = root.AddComponent<OpeningSequence>();
            sequence.Configure(
                skipObject.GetComponent<Button>(),
                line,
                new[] { "첫 문장", "둘째 문장", "셋째 문장", "넷째 문장" },
                0.03f,
                "03.Mouse");
            root.SetActive(true);

            Assert.That(line.text, Is.EqualTo(string.Empty));
            yield return null;
            Assert.That(line.text, Is.EqualTo("첫 문장"));

            var timeout = Time.realtimeSinceStartup + 0.5f;
            while (!line.text.Contains("넷째 문장") && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(line.text, Is.EqualTo("첫 문장\n둘째 문장\n셋째 문장\n넷째 문장"));
            Object.Destroy(root);
            yield return null;
        }
    }
}
