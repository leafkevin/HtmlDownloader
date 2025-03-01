using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HtmlRefactor;

class HtmlFormater
{
    public string Transfer(string codeScripts)
    {
        using var reader = new StringReader(codeScripts);
        codeScripts = this.TrimWhiteSpace(reader);
        reader.Close();
        return codeScripts;
    }
    private string TrimWhiteSpace(StringReader reader)
    {
        var lines = new StringBuilder();
        var tagRules = new List<TagRule>
        {
            new TagRule
            {
                BeginTag = "<",
                EndTag = ">",
                IsClosed = true,
                IsAllowBeginTagLeftWhiteSpace = true,
                IsAllowBeginTagRightWhiteSpace = false,
                IsAllowEndTagLeftWhiteSpace = false,
                IsAllowEndTagRightWhiteSpace = false,
                IsAllowTextWhiteSpace = false,
                IsNeedToSpace = false
            },
            new TagRule
            {
                BeginTag = "<",
                EndTag = "\"",
                IsClosed = false,
                IsAllowBeginTagLeftWhiteSpace = true,
                IsAllowBeginTagRightWhiteSpace = false,
                IsAllowEndTagLeftWhiteSpace = false,
                IsAllowEndTagRightWhiteSpace = false,
                IsAllowTextWhiteSpace = true,
                IsNeedToSpace = true
            },
            new TagRule
            {
                BeginTag = "\"",
                EndTag = "\"",
                IsClosed = true,
                IsAllowBeginTagLeftWhiteSpace = false,
                IsAllowBeginTagRightWhiteSpace = false,
                IsAllowEndTagLeftWhiteSpace = false,
                IsAllowEndTagRightWhiteSpace = true,
                IsAllowTextWhiteSpace = true,
                IsNeedToSpace = true
            },
            new TagRule
            {
                BeginTag = "\"",
                EndTag = ">",
                IsClosed = false,
                IsAllowBeginTagLeftWhiteSpace = false,
                IsAllowBeginTagRightWhiteSpace = false,
                IsAllowEndTagLeftWhiteSpace = false,
                IsAllowEndTagRightWhiteSpace = false,
                IsAllowTextWhiteSpace = false,
                IsNeedToSpace = false
            },
            new TagRule
            {
                BeginTag = ">",
                EndTag = "<",
                IsClosed = false,
                IsAllowBeginTagLeftWhiteSpace = false,
                IsAllowBeginTagRightWhiteSpace = false,
                IsAllowEndTagLeftWhiteSpace = false,
                IsAllowEndTagRightWhiteSpace = false,
                IsAllowTextWhiteSpace = true,
                IsNeedToSpace = false
            }
        };
        var tags = new List<string> { "<", "\"", ">" };
        var trimTags = new List<string> { "=" };
        string lastCharacter = null;
        string lastTag = null, myLastTag = null;
        bool hasCharacter = false;
        var lineBuilder = new StringBuilder();
        var builder = new StringBuilder();
        var ruleTags = new Stack<string>();
        TagRule myRule = null;
        int nextTagIndex = -1;

        while (reader.Peek() > -1)
        {
            var lineText = reader.ReadLine();
            for (int i = 0; i < lineText.Length; i++)
            {
                var character = lineText[i].ToString();
                if (string.IsNullOrEmpty(lastTag))
                {
                    if (char.IsWhiteSpace(character, 0))
                        continue;
                    builder.Append(character);
                    lastCharacter = character;
                    lastTag = character;
                    ruleTags.Push(character);
                    continue;
                }
                else if (!ruleTags.TryPeek(out myLastTag))
                {
                    if (char.IsWhiteSpace(character[0]))
                    {
                        //新开标签或是同行><标签间的文本
                        if (nextTagIndex > 0)
                        {
                            //如果前面有普通字符，>  xxx xxx < 标签内文本的场景
                            if (hasCharacter) builder.Append(character);
                            //临时添加一个空格，在下一个标签时，再处理是trim掉，还是保留
                            else if (!char.IsWhiteSpace(lastCharacter[0]))
                                builder.Append(' ');
                        }
                        else
                        {
                            //换行了，添加前导缩进空白符
                            builder.Append(character);
                            lastCharacter = character;
                        }
                        continue;
                    }
                    //同行，不是>  xxx xxx < 标签内文本的场景，去掉左侧空白符
                    if (!hasCharacter && nextTagIndex > 0)
                    {
                        while (builder.Length > 0)
                        {
                            var index = builder.Length - 1;
                            var myCharacter = builder[index];
                            if (!char.IsWhiteSpace(myCharacter))
                                break;
                            builder.Remove(index, 1);
                        }
                    }
                    builder.Append(character);
                    lastCharacter = character;
                    if (this.IsTag(character, tags))
                    {
                        lastTag = character;
                        ruleTags.Push(character);
                        hasCharacter = false;
                    }
                    else hasCharacter = true;
                    continue;
                }
                //找匹配规则行文本
                if (i > nextTagIndex)
                {
                    if (this.TryFindFirstTag(tags, lineText, i, out var nextTag, out nextTagIndex))
                        myRule = tagRules.Find(f => f.BeginTag == (myLastTag ?? lastTag) && f.EndTag == nextTag);
                    else
                    {
                        lineBuilder.Clear();
                        lineBuilder.AppendLine(lineText);
                        while (true)
                        {
                            var endIndex = lineBuilder.Length;
                            var myLine = reader.ReadLine(); ;
                            if (string.IsNullOrEmpty(myLine.Trim())) continue;
                            lineBuilder.AppendLine(myLine);
                            lineText = lineBuilder.ToString();
                            if (this.TryFindFirstTag(tags, lineText, endIndex, out nextTag, out nextTagIndex))
                            {
                                myRule = tagRules.Find(f => f.BeginTag == (myLastTag ?? lastTag) && f.EndTag == nextTag);
                                lineBuilder.Clear();
                                break;
                            }
                        }
                    }
                }

                //已经有普通字符了，说明就支持IsAllowTextWhiteSpace
                if (hasCharacter)
                {
                    //<a __, <a href="xxx" __, <a class="abc __, ...> xxx xxx <..., > __
                    if (char.IsWhiteSpace(character[0]))
                    {
                        //如果没有在当前行找到下一个标签，就当作IsAllowInlineWhiteSpace=True处理，在下一个标签再处理
                        if (!myRule.IsNeedToSpace)
                            builder.Append(character);
                        else if (!char.IsWhiteSpace(lastCharacter[0]))
                            builder.Append(' ');
                        lastCharacter = character;
                    }
                    //标签
                    //<a href=, <a href __, <a href=, > xxxx 
                    else if (this.IsTag(character, tags))
                    {
                        //所有第一个标签以后的标签，都不允许左侧有空白符，删除左侧所有空白符
                        if (!myRule.IsAllowEndTagLeftWhiteSpace)
                        {
                            while (builder.Length > 0)
                            {
                                var index = builder.Length - 1;
                                var myCharacter = builder[index];
                                if (!char.IsWhiteSpace(myCharacter))
                                    break;
                                builder.Remove(index, 1);
                            }
                        }
                        builder.Append(character);
                        lastCharacter = character;

                        //处理配对
                        hasCharacter = false;
                        lastTag = character;
                        //处理配对
                        if (myRule.IsClosed && i >= nextTagIndex)
                        {
                            ruleTags.TryPop(out _);
                            if (ruleTags.Count > 0)
                                hasCharacter = true;
                        }
                        else ruleTags.Push(character);
                    }
                    else
                    {
                        //删除需要Trim字符左侧的所有空白符
                        if (this.IsTrimTag(character, trimTags))
                        {
                            //所有标签右侧都不愿徐有空白符，删除标签右侧所有空白符
                            while (builder.Length > 0)
                            {
                                var index = builder.Length - 1;
                                var myCharacter = builder[index];
                                if (!char.IsWhiteSpace(myCharacter))
                                {
                                    lastCharacter = myCharacter.ToString();
                                    break;
                                }
                                builder.Remove(index, 1);
                            }
                        }
                        builder.Append(character);
                        lastCharacter = character;
                        hasCharacter = true;
                    }
                }
                else
                {
                    if (char.IsWhiteSpace(character[0]))
                    {
                        if (myRule.IsAllowBeginTagRightWhiteSpace)
                        {
                            builder.Append(character);
                            lastCharacter = character;
                            continue;
                        }

                        while (builder.Length > 0)
                        {
                            var index = builder.Length - 1;
                            var myCharacter = builder[index];
                            if (!char.IsWhiteSpace(myCharacter))
                                break;
                            builder.Remove(index, 1);
                        }
                        //跳过空白符
                        continue;
                    }

                    //普通字符或可以连续的标签，直接添加
                    builder.Append(character);
                    lastCharacter = character;

                    if (this.IsTag(character, tags))
                    {
                        hasCharacter = false;
                        lastTag = character;
                        //处理配对
                        if (myRule.IsClosed && i >= nextTagIndex)
                        {
                            ruleTags.TryPop(out _);
                            if (ruleTags.Count > 0)
                                hasCharacter = true;
                        }
                        else ruleTags.Push(character);
                    }
                    else hasCharacter = true;
                }
            }
            lines.AppendLine(builder.ToString());
            builder.Clear();
            nextTagIndex = -1;
        }
        return lines.ToString();
    }
    private bool IsTag(string character, List<string> tags) => tags.Contains(character);
    private bool IsTrimTag(string character, List<string> tags) => tags.Contains(character);
    private bool TryFindFirstTag(List<string> tags, string lineText, int startIndex, out string tag, out int index)
    {
        for (int i = startIndex; i < lineText.Length; i++)
        {
            var character = lineText[i].ToString();
            if (tags.Contains(character))
            {
                tag = character;
                index = i;
                return true;
            }
        }
        index = -1;
        tag = null;
        return false;
    }
    class TagRule
    {
        public string BeginTag { get; set; }
        public string EndTag { get; set; }
        public bool IsClosed { get; set; }
        public bool IsAllowBeginTagLeftWhiteSpace { get; set; }
        public bool IsAllowBeginTagRightWhiteSpace { get; set; }
        public bool IsAllowEndTagLeftWhiteSpace { get; set; }
        public bool IsAllowEndTagRightWhiteSpace { get; set; }
        /// <summary>
        /// 只在存在上一个标签，并且上一个字符是普通字符时，此值有效
        /// </summary>
        public bool IsAllowTextWhiteSpace { get; set; }
        public bool IsNeedToSpace { get; set; }
    }
}
