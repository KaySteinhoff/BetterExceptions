using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BetterExceptions.Web
{
	public class HTMLDoc
	{
		private class HTMLElement
		{
			public int sequenceID { get; }
			public string name { get; }
			public object content { get; internal set; } = null;
			public SortedDictionary<int, KeyValuePair<string, object>> attributes { get; } = new SortedDictionary<int, KeyValuePair<string, object>>();
			public HTMLElement parent;

			internal HTMLElement(int sequenceID, string name, HTMLElement parent)
			{
				this.sequenceID = sequenceID;
				this.name = name;
				this.parent = parent;
			}
		}
		private SortedDictionary<int, HTMLElement> elements = new SortedDictionary<int, HTMLElement>();

		private HTMLElement currentElement = null;

		public HTMLDoc()
		{ }

		public void OpenElement(int sequence, string name)
		{
			if(elements.ContainsKey(sequence))
				throw new ArgumentException("An element with that sequence position already exists!");

			currentElement = new HTMLElement(sequence, name, currentElement);
			elements.Add(sequence, currentElement); 
		}

		public void AddAttribute(int sequence, string id, object value)
		{
			if(currentElement == null)
				throw new InvalidOperationException("Sequence contains no element to add an attribute to!");

			if(currentElement.attributes.ContainsKey(sequence))
				throw new ArgumentException("There already exists an attibute at that sequence position!");

			currentElement.attributes.Add(sequence, new KeyValuePair<string, object>(id, value));
		}

		public void AddContent(int idx, object value)
		{
			if(currentElement == null)
				throw new InvalidOperationException("No opened element to add content to!");
			if(elements.ContainsKey(idx))
				throw new ArgumentException("An element with that sequence position already exists!");
            
			// We utilize a new HTMLElement instance as we can use it's position
			// in the list to figure out when to inset it into the html file
			HTMLElement contentElement = new HTMLElement(idx, null, currentElement);
			contentElement.content = value;
			elements.Add(idx, contentElement);
		}

		public void CloseElement()
		{
			if(currentElement == null)
				throw new InvalidOperationException("Sequence contains no element to close!");

			currentElement = currentElement.parent;
		}

		private void ConvertToString(StringBuilder stringBuilder, ref int i)
		{
			int rootKey = elements.Keys.ElementAt(i);
			HTMLElement root = elements[rootKey];
			if(root.name != null && root.name.Length > 0)
			{
				stringBuilder.Append($"<{root.name}");
				for(int j = 0; j < root.attributes.Count; ++j)
				{
					int attrKey = root.attributes.Keys.ElementAt(j);
					stringBuilder.Append($" {root.attributes[attrKey].Key}=\"{root.attributes[attrKey].Value}\"");
				}

				stringBuilder.Append(">");
			}
			else
			{
				if(root.content != null)
					stringBuilder.Append(root.content.ToString());
				return;
			}

			while(i < elements.Keys.Count - 1 && elements[elements.Keys.ElementAt(i + 1)].parent == root)
			{
				i++;
				ConvertToString(stringBuilder, ref i);
			}

			if(root.name != null && root.name.Length > 0)
				stringBuilder.Append($"</{root.name}>");
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			// We only want to convert "root" elements here
			// where root element is defined as an element that has no parent
			for(int i = 0; i < elements.Keys.Count && elements[elements.Keys.ElementAt(i)].parent == null; ++i)
				ConvertToString(stringBuilder, ref i);

			return stringBuilder.ToString();
		}
	}
}
