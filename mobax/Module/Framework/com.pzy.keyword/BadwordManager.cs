using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;


public class BadwordManager : StuffObject<BadwordManager>
{
    [ShowInInspector]
	Dictionary<int, CharNode> indexToRootDic = new Dictionary<int, CharNode>();

	public CharNode GetOrCreateRoot(int index)
    {
		indexToRootDic.TryGetValue(index, out CharNode root);
		if(root == null)
        {
			root = new CharNode();
			indexToRootDic[index] = root;
		}
		return root;
	}

	public void AddWorld(int libraryIndex, string world)
    {
		var root = GetOrCreateRoot(libraryIndex);
		var p = root;
		foreach (var c in world)
        {
			p = p.GetOrCreateFollowing(c);
		}
		p.canAsEnd = true;
    }


	/// <summary>
	/// 单词测试
	/// </summary>
	/// <returns></returns>
	bool TestWord(int libraryIndex, string worldContainer, int startIndex, int endIndexInclusive, out bool falseWithCanNotAsEnd, out bool trueButCanHasFollow)
	{
		falseWithCanNotAsEnd = false;
		trueButCanHasFollow = false;
		var root = GetOrCreateRoot(libraryIndex);
		var p = root;
		for(int i = startIndex; i <= endIndexInclusive; i++)
		{
			var c = worldContainer[i];
			p = p.GetFollow(c);
			if (p == null)
			{
				return false;
			}
		}
		if (p.canAsEnd)
		{
			var hasFollow = p.HasFollow();
			trueButCanHasFollow = hasFollow;
			return true;
		}
		else
        {
			falseWithCanNotAsEnd = true;
			return false;
		}
	}

	int FindFirstNonBlankChar(string text, int searchStartIndex)
    {
		var index = searchStartIndex;
		if(index >= text.Length)
        {
			return -1;
        }
		var c = text[index];
		while(c == ' ')
        {
			index++;
			if(index >= text.Length)
            {
				return -1;
            }
			c = text[index];
		}
		return index;
	}

	int FindFirstLatestNonBlankCharIndex(string text, int searchStartIndex)
	{
		var index = searchStartIndex;
		if (index >= text.Length)
		{
			return -1;
		}
		var c = text[index];
		while (c != ' ')
		{
			here:
			var tryIndex = index + 1;
			if (tryIndex >= text.Length)
			{
				break;
			}
			c = text[tryIndex];
			if(c != ' ')
            {
				index++;
				goto here;
			}
		}
		return index;
	}

	public List<MatchInfo> Match(int libraryIndex, string text, MatchType matchType)
    {
		if(matchType == MatchType.Fuzzy)
		{
			var ret = FuzzyMatch(libraryIndex, text);
			return ret;
		}
		else if(matchType == MatchType.HoleWord)
        {
			var ret = HoleWordMatch(libraryIndex, text);
			return ret;
        }
		else
        {
			throw new Exception("not implement");
        }
    }

	List<MatchInfo> HoleWordMatch(int libraryIndex, string text)
	{
		var ret = new List<MatchInfo>();
		for (int i = 0; i < text.Length; i++)
		{
			var wordStart = FindFirstNonBlankChar(text, i);
			if(wordStart == -1)
            {
				return ret;
            }
			var wordEnd = FindFirstLatestNonBlankCharIndex(text, wordStart);

			var b = this.TestWord(libraryIndex, text, wordStart, wordEnd, out var falseWithCanNotAsEnd, out var trueButCanHasFollow);
			if(b)
            {
				var info = new MatchInfo();
				info.startIndex = wordStart;
				info.endIndex = wordEnd;
				ret.Add(info);
			}
			i = wordEnd + 1;
		}
		return ret;
	}

	List<MatchInfo> FuzzyMatch(int libraryIndex, string text)
	{
		var ret = new List<MatchInfo>();
		for(int i = 0; i < text.Length; i++)
        {
			var wordStart = i;
			var wordEnd = wordStart;
			var b = this.TestWord(libraryIndex, text, wordStart, wordEnd, out var falseWithCanNotAsEnd, out var trueButCanHasFollow);
			if(b)
            {
				var testEnd = wordEnd;
				while (b && trueButCanHasFollow)
                {
					testEnd ++;
					if(testEnd >= text.Length)
                    {
						break;
                    }
					b = this.TestWord(libraryIndex, text, wordStart, testEnd, out falseWithCanNotAsEnd, out trueButCanHasFollow);
					if(b)
                    {
						wordEnd = testEnd;
					}
				}
				var info = new MatchInfo();
				info.startIndex = wordStart;
				info.endIndex = wordEnd;
				ret.Add(info);
				i = wordEnd;
			}
			else
            {
				var testEnd = wordEnd;
				while (!b && falseWithCanNotAsEnd)
				{
					testEnd++;
					if (testEnd >= text.Length)
					{
						break;
					}
					b = this.TestWord(libraryIndex, text, wordStart, testEnd, out falseWithCanNotAsEnd, out trueButCanHasFollow);
					if (b)
					{
						wordEnd = testEnd;
					}
				}
				if(b)
                {
					var info = new MatchInfo();
					info.startIndex = wordStart;
					info.endIndex = wordEnd;
					ret.Add(info);
					i = wordEnd;
				}

			}
		}
		return ret;
    }
}

public enum MatchType
{
	/// <summary>
	/// 整词匹配
	/// </summary>
	HoleWord,

	/// <summary>
	/// 模糊匹配
	/// </summary>
	Fuzzy,
}

public struct MatchInfo
{
	public int startIndex;
	public int endIndex;
}

public class CharNode
{
	public readonly char c;
	public Dictionary<char, CharNode> followingCharToNodeDic;
	public bool canAsEnd;

	public CharNode()
	{
		
	}

	public CharNode(char c)
    {
        this.c = c;
    }

	public bool HasFollow()
    {
		if(followingCharToNodeDic == null)
        {
			return false;
        }
		var count = followingCharToNodeDic.Count;
		if(count > 0)
        {
			return true;
        }
		return false;
	}

	void CreateFollowDicIfNeed()
    {
		if (this.followingCharToNodeDic == null)
		{
			this.followingCharToNodeDic = new Dictionary<char, CharNode>();
		}
	}

	// 英文忽略大小写
	const bool IGNORE_CASE = true;

    public CharNode GetOrCreateFollowing(char c)
    {
		if (IGNORE_CASE)
		{
			c = Char.ToLower(c);
		}
		this.CreateFollowDicIfNeed();
		followingCharToNodeDic.TryGetValue(c, out var node);
		if(node == null)
        {
			node = new CharNode(c);
			followingCharToNodeDic[c] = node;
		}
		return node;
	}

	public CharNode GetFollow(char c)
    {
		if(IGNORE_CASE)
        {
			c = Char.ToLower(c);
        }
		if(this.followingCharToNodeDic == null)
        {
			return null;
        }
		followingCharToNodeDic.TryGetValue (c, out var node);
		return node;
	}
}
