using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class BasePager
    {
        // 当前页从1开始
        protected int _currentPage;
        protected int _totalPage;
        protected int _pageSize;
        
        public int CurrentPage => _currentPage;
        public int TotalPage => _totalPage;

        public virtual bool TurnTo(int page)
        {
            return false;
        }

        public virtual bool TurnOffset(int offset)
        {
            return false;
        }
    }
    
    public class Pager<T> : BasePager
    {
        private List<T> _list;
        public List<T> List => _list;

        public uint DisplayNum
        {
            get
            {
                if (_currentPage < _totalPage)
                {
                    return (uint) _pageSize;
                }

                if (_list.Count <= 0)
                {
                    return 0;
                }

                return (uint) (_list.Count - (_currentPage - 1) *  _pageSize);
            }
        }

        public Pager(int pageSize)
        {
            ResetPageSize(pageSize);
        }

        public Pager(int pageSize, List<T> list)
        {
            ResetPageSize(pageSize);
            RefreshList(list);
        }

        public void ResetPageSize(int pageSize)
        {
            _pageSize = pageSize;
        }

        public void RefreshList(List<T> list)
        {
            _list = list;
            RefreshList();
        }

        public void RefreshList()
        {
            _totalPage = (int) Mathf.Ceil((float)_list.Count / _pageSize);
            if (_totalPage <= 0)
            {
                _currentPage = 0;
            }
            else if (_currentPage > _totalPage)
            {
                _currentPage = _totalPage;
            }
            else if (_currentPage <= 0)
            {
                _currentPage = 1;
            }
        }

        /** 查看item在当前页的索引，不在当前页就返回-1 */
        public int GetIndex(T item)
        {
            var indexInList = _list.IndexOf(item);
            if (indexInList < 0)
            {
                return -1;
            }
            
            var index = indexInList - (_currentPage - 1) * _pageSize;
            if (index >= 0 && index < _pageSize)
            {
                return index;
            }

            return -1;
        }

        /** 找到当前item在列表中的第几页 */
        public int GetPage(T item)
        {
            var indexInList = _list.IndexOf(item);
            if (indexInList < 0)
            {
                return 0;
            }

            return indexInList / _pageSize + 1;
        }

        /** 获取当前页指定索引的元素 */
        public T GetItem(int index)
        {
            var realIndex = (_currentPage - 1) * _pageSize + index;
            return _list[realIndex];
        }

        public void ResetPage()
        {
            TurnTo(1);
        }

        /** 返回是否成功翻页 */
        public override bool TurnTo(int page)
        {
            if (page > 0 && page <= _totalPage)
            {
                _currentPage = page;
                return true;
            }

            if (page <= 0)
            {
                _currentPage = 1;
            }
            else if (page > _totalPage)
            {
                _currentPage = _totalPage;
            }

            return false;
        }

        public override bool TurnOffset(int offset)
        {
            return TurnTo(CurrentPage + offset);
        }
    }
}