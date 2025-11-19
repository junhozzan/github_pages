using System;
using System.Collections.Generic;
using System.Xml;

public readonly struct TXMLAttribute 
{
    public readonly string name;
    public readonly string text;

    public TXMLAttribute(string name, string text)
    {
        this.name = name;
        this.text = text;
    }
}

public class TXMLElement 
{
    public readonly string name = string.Empty;
    public string text { get; private set; } = string.Empty;

    // 동일 요소(키) 없음
    public readonly Dictionary<string, TXMLAttribute> nattributes = null;
    public readonly Dictionary<string, List<TXMLElement>> nchildren = new();

    public static readonly TXMLElement empty = new(string.Empty, null);

    public TXMLElement(string name, Dictionary<string, TXMLAttribute> nattributes)
    {
        this.name = name;
        this.nattributes = nattributes;
    }

    public void AddChild(TXMLElement e)
    {
        if (!nchildren.TryGetValue(e.name, out var v))
        {
            nchildren.Add(e.name, v = new());
        }

        v.Add(e);
    }

    public void SetText(string s)
    {
        text = s;
    }
}

public class SmartXmlNode : IDisposable
{
    // 동일 요소(키) 없음, 아이디 정보 획득
    private Dictionary<string, TXMLAttribute> _fastAttributes = null;
    private Dictionary<string, List<TXMLElement>> _fastChildren = new();

    public SmartXmlNode(XmlReader reader)
    {
        // 1. SmartXmlNode 자체의 속성 파싱 및 ID 설정
        _fastAttributes = ParseNodeAttributes(reader);

        if (reader.IsEmptyElement)
        {
            return;
        }

        // 2. 자식 요소 파싱
        ParseChildElements(reader, _fastChildren);
    }

    /// <summary>
    /// 현재 XmlReader 위치의 노드 속성을 파싱하여 _fastAttributes에 추가하고, ID를 설정합니다.
    /// </summary>
    private Dictionary<string, TXMLAttribute> ParseNodeAttributes(XmlReader reader)
    {
        if (!reader.HasAttributes)
        {
            return null;
        }

        var result = new Dictionary<string, TXMLAttribute>();
        reader.MoveToFirstAttribute();
        do
        {
            var attr = new TXMLAttribute(reader.Name, reader.Value);
            // 중복 키 입력 방지를 위해 예외처리 제외.
            result.Add(attr.name, attr);
        } 
        while (reader.MoveToNextAttribute());
        reader.MoveToElement(); // 속성을 읽은 후 다시 요소로 돌아와야 합니다.

        return result;
    }

    /// <summary>
    /// 현재 XmlReader 위치의 자식 요소들을 파싱하여 _fastChildren에 추가합니다.
    /// </summary>
    private void ParseChildElements(XmlReader reader, Dictionary<string, List<TXMLElement>> fastChildren)
    {
        Stack<TXMLElement> elementStack = new Stack<TXMLElement>();
        TXMLElement currentParent = null; // 현재 처리 중인 부모 요소

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    HandleElementNode(reader, fastChildren, elementStack, ref currentParent);
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    HandleTextNode(reader, currentParent);
                    break;

                case XmlNodeType.EndElement:
                    // 현재 SmartXmlNode의 닫는 태그를 만났으면 파싱 종료
                    if (elementStack.Count == 0)
                    {
                        return;
                    }
                    HandleEndElementNode(elementStack, ref currentParent);
                    break;

                // 필요한 경우 다른 노드 유형 처리
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                    // 무시하거나 필요에 따라 처리
                    break;

                default:
                    // 알 수 없는 노드 유형에 대한 처리 (로깅 등)
                    break;
            }
        }
    }

    /// <summary>
    /// 특정 요소의 속성을 파싱하여 List<TXMLAttribute>에 추가합니다.
    /// </summary>
    private Dictionary<string, TXMLAttribute> ParseAndAddElementAttributes(XmlReader reader)
    {
        if (!reader.HasAttributes)
        {
            return null;
        }

        var result = new Dictionary<string, TXMLAttribute>();
        reader.MoveToFirstAttribute();
        do
        {
            var attr = new TXMLAttribute(reader.Name, reader.Value);
            // 중복 입력 방지를 위해 예외처리 제거.
            result.Add(attr.name, attr);
        }
        while (reader.MoveToNextAttribute());
        reader.MoveToElement(); // 중요: 속성을 모두 읽은 후 요소로 돌아와야 함

        return result;
    }

    /// <summary>
    /// XmlNodeType.Element를 처리합니다.
    /// </summary>
    private void HandleElementNode(XmlReader reader, Dictionary<string, List<TXMLElement>> fastChildren,
                                   Stack<TXMLElement> elementStack, ref TXMLElement currentParent)
    {
        bool isEmptyElement = reader.IsEmptyElement;
        var newElement = new TXMLElement(reader.Name, ParseAndAddElementAttributes(reader));

        // 현재 부모에 따라 자식 목록에 추가
        if (currentParent == null)
        {
            // SmartXmlNode의 직계 자식
            if (!fastChildren.TryGetValue(newElement.name, out var childList))
            {
                fastChildren.Add(newElement.name, childList = new List<TXMLElement>());
            }

            childList.Add(newElement);
        }
        else
        {
            // 다른 요소의 자식
            currentParent.AddChild(newElement);
        }

        if (!isEmptyElement)
        {
            elementStack.Push(newElement);
            currentParent = newElement; // 새 요소가 다음 부모가 됨
        }
    }

    /// <summary>
    /// XmlNodeType.Text 또는 XmlNodeType.CDATA를 처리합니다.
    /// </summary>
    private void HandleTextNode(XmlReader reader, TXMLElement currentParent)
    {
        currentParent?.SetText(reader.Value);
    }

    /// <summary>
    /// XmlNodeType.EndElement를 처리합니다.
    /// </summary>
    private void HandleEndElementNode(Stack<TXMLElement> elementStack, ref TXMLElement currentParent)
    {
        elementStack.Pop();
        currentParent = elementStack.Count > 0 ? elementStack.Peek() : null;
    }

    private static readonly List<TXMLElement> emptyList = new();
    public List<TXMLElement> GetChildren(string key)
    {
        if (!_fastChildren.TryGetValue(key, out var lst))
        {
            return emptyList;
        }

        return lst;
    }

    public TXMLElement GetChild(string key)
    {
        if (!_fastChildren.TryGetValue(key, out var lst) || lst.Count == 0)
        {
            return null;
        }

        return lst[0];
    }

    public string GetChildText(string key, string _default = null)
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return child.text;
    }

    public string GetChildContent(string key, string _default = null)
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return child.text.Replace("\\n", "\n");
    }

    public T GetChildEnum<T>(string key, T _default = default) where T : struct
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return (T)Enum.Parse(typeof(T), child.text);
    }

    public bool GetChildBoolean(string key, bool _default = false)
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return bool.Parse(child.text);
    }

    public int GetChildInt(string key, int _default = 0)
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return int.Parse(child.text);
    }

    public long GetChildLong(string key, long _default = 0)
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return long.Parse(child.text);
    }

    public float GetChildFloat(string key, float _default = 0f)
    {
        var child = GetChild(key);
        if (child == null)
        {
            return _default;
        }

        return float.Parse(child.text);
    }

    /// <summary>
    /// 아이디 파싱 전용
    /// </summary>
    public int GetAttributeInt(string key, int _default = 0)
    {
        if (!_fastAttributes.TryGetValue(key, out var v))
        {
            return _default;
        }

        return int.Parse(v.text);
    }

    void IDisposable.Dispose()
    {
        _fastAttributes?.Clear();
        _fastChildren?.Clear();
    }
}

public static class SmartXmlNodeExtensions
{

    public static TXMLElement GetChild(this TXMLElement node, string key)
    {
        if (!node.nchildren.TryGetValue(key, out var v) || v.Count == 0)
        {
            return null;
        }

        return v[0];
    }

    public static string GetChildText(this TXMLElement node, string key, string _default = null)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return child.text;
    }

    public static T GetChildEnum<T>(this TXMLElement node, string key, T _default = default) where T : struct
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return (T)Enum.Parse(typeof(T), child.text);
    }

    /// <summary>
    /// 문장을 반환 할때 사용
    /// </summary>
    public static string GetChildContent(this TXMLElement node, string key, string _default = null)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return child.text.Replace("\\n", "\n");
    }

    public static bool GetChildBoolean(this TXMLElement node, string key, bool _default = false)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return bool.Parse(child.text);
    }

    public static int GetChildInt(this TXMLElement node, string key, int _default = 0)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return int.Parse(child.text);
    }

    public static long GetChildLong(this TXMLElement node, string key, long _default = 0)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return long.Parse(child.text);
    }

    public static float GetChildFloat(this TXMLElement node, string key, float _default = 0f)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return float.Parse(child.text);
    }

    public static double GetChildDouble(this TXMLElement node, string key, double _default = 0d)
    {
        var child = GetChild(node, key);
        if (child == null)
        {
            return _default;
        }

        return double.Parse(child.text);
    }

    private static readonly List<TXMLElement> emptyList = new();
    public static List<TXMLElement> GetChildren(this TXMLElement node, string key)
    {
        if (!node.nchildren.TryGetValue(key, out var v) || v.Count == 0)
        {
            return emptyList;
        }

        return v;
    }

    private static TXMLAttribute? GetAttribute(this TXMLElement node, string key)
    {
        if (node.nattributes == null || !node.nattributes.TryGetValue(key, out var v))
        {
            return null;
        }

        return v;
    }

    public static string GetAttributeText(this TXMLElement node, string key, string _default = null)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return attr.Value.text;
    }

    public static string GetAttributeContent(this TXMLElement node, string key, string _default = null)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return attr.Value.text.Replace("\\n", "\n");
    }

    public static T GetAttributeEnum<T>(this TXMLElement node, string key, T _default = default) where T : struct
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }
        
        return (T)Enum.Parse(typeof(T), attr.Value.text);
    }

    public static int GetAttributeInt(this TXMLElement node, string key, int _default = 0)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return int.Parse(attr.Value.text);
    }

    public static bool GetAttributeBoolean(this TXMLElement node, string key, bool _default = false)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return bool.Parse(attr.Value.text);
    }

    public static float GetAttributeFloat(this TXMLElement node, string key, float _default = 0)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return float.Parse(attr.Value.text);
    }

    public static double GetAttributeDouble(this TXMLElement node, string key, double _default = 0)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return double.Parse(attr.Value.text);
    }

    public static long GetAttributeLong(this TXMLElement node, string key, long _default = 0)
    {
        var attr = GetAttribute(node, key);
        if (!attr.HasValue)
        {
            return _default;
        }

        return long.Parse(attr.Value.text);
    }
}