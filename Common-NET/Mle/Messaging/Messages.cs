
namespace Mle.Messaging {
    public class HeaderMessage : SimpleMessage {
        public string Title { get; private set; }

        public HeaderMessage(string title, string content)
            : base(content) {
            Title = title;
        }
    }

    public class NeverAgainOptionMessage : SimpleMessage {
        public string PreferenceKey { get; private set; }
        public NeverAgainOptionMessage(string content, string prefKey)
            : base(content) {
            PreferenceKey = prefKey;
        }
    }

    public class SimpleMessage {
        public string Content { get; private set; }

        public SimpleMessage(string content) {
            Content = content;
        }
    }
}
