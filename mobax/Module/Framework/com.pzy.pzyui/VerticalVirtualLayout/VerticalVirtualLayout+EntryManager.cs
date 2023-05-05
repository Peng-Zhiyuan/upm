using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public partial class VerticalVirtualLayout 
{
    public class EntryManager
    {
        [ShowInInspector]
        List<DataEntry> dataEntryList = new List<DataEntry>();
        public List<RectTransform> Reset(IList dataList)
        {
            var viewList = this.RemoveAllView();
            dataEntryList.Clear();
            if(dataList != null)
            {
                foreach (var data in dataList)
                {
                    var entry = CreateEntry(data);
                    dataEntryList.Add(entry);
                }
            }
            return viewList;
        }

        public object GetData(int dataIndex)
        {
            var isValid = IsDataIndexValid(dataIndex);
            if(!isValid)
            {
                throw new Exception($"[VerticalVirtualLayout] dataIndex {dataIndex} not valid. (count: {this.Count})");
            }
            return this.dataEntryList[dataIndex].data;
        }

        public DataEntry GetEntry(int dataIndex)
        {
            return dataEntryList[dataIndex];
        }

        public RectTransform RemoveAt(int dataIndex)
        {
            RectTransform ret = null;
            var entry = GetEntry(dataIndex);
            if(entry.HasView)
            {
                ret = this.RemoveView(dataIndex);
            }
            dataEntryList.RemoveAt(dataIndex);
            if(this.ViewCount == 0)
            {
                this.viewStartIndex = -1;
                this.viewEndIndex = -1;
            }
            else
            {
                if (this.viewStartIndex > dataIndex)
                {
                    this.viewStartIndex--;
                }
                if (this.viewEndIndex > dataIndex)
                {
                    this.viewEndIndex--;
                }
            }
            return ret;
        }

        [ShowInInspector, ReadOnly]
        public int Count
        {
            get
            {
                return dataEntryList.Count;
            }
        }

        public int LastDataIndex => Count - 1;

        public void Add(object data)
        {
            var entry = CreateEntry(data);
            this.dataEntryList.Add(entry);
        }

        DataEntry CreateEntry(object data)
        {
            var entry = new DataEntry();
            entry.data = data;
            return entry;
        }

        [ShowInInspector, ReadOnly]
        int viewStartIndex = -1;

        [ShowInInspector, ReadOnly]
        int viewEndIndex = -1;

        [ShowInInspector, ReadOnly]
        int viewCount;

        public void SetView(int dataIndex, RectTransform additionView)
        {
            var entry = GetEntry(dataIndex);
            if(entry.view != null)
            {
                throw new Exception($"[VirtualVirtualLayout] data of index {dataIndex} already has a view");
            }
            entry.view = additionView;

            if (this.viewStartIndex == -1 && this.viewEndIndex == -1)
            {
                this.viewStartIndex = dataIndex;
                this.viewEndIndex = dataIndex;
            }
            else if (dataIndex > this.viewEndIndex)
            {
                this.viewEndIndex = dataIndex;
            }
            else if (dataIndex < this.viewStartIndex)
            {
                this.viewStartIndex = dataIndex;
            }
            viewCount++;
        }

        public void SetViewRange(int startDataIndex, List<RectTransform> viewList)
        {
            var count = viewList.Count;
            var endDataIndexExclusive = startDataIndex + count;
            var j = 0;
            for(int index = startDataIndex; index < endDataIndexExclusive; index++)
            {
                var view = viewList[j];
                j++;
                SetView(index, view);
            }
        }

        public RectTransform RemoveView(int dataIndex)
        {
            var entry = GetEntry(dataIndex);
            if (entry.view == null)
            {
                throw new Exception($"[VirtualVirtualLayout] data of index {dataIndex} has no view");
            }
            var view = entry.view;
            entry.view = null;

            viewCount--;
            if(viewCount == 0)
            {
                this.viewStartIndex = -1;
                this.viewEndIndex = -1;
            }
            else
            {
                if (this.viewStartIndex == dataIndex)
                {
                    var testIndex = this.viewStartIndex + 1;
                    while (!HasView(testIndex))
                    {
                        testIndex++;
                    }
                    this.viewStartIndex = testIndex;
                }
                else if (this.viewEndIndex == dataIndex)
                {
                    var testIndex = this.viewEndIndex - 1;
                    while (!HasView(testIndex))
                    {
                        testIndex--;
                    }
                    this.viewEndIndex = testIndex;
                }
            }
            return view;

        }

        public List<RectTransform> RemoveAllView()
        {
            var ret = new List<RectTransform>();
            if(ViewCount == 0)
            {
                return ret;
            }
            for(int i = this.viewStartIndex; i <= this.viewEndIndex; i++)
            {
                var entry = this.GetEntry(i);
                var view = entry.view;
                if(view != null)
                {
                    ret.Add(view);
                    entry.view = null;
                }
            }
            this.viewCount = 0;
            this.viewStartIndex = -1;
            this.viewEndIndex = -1;
            return ret;
        }

        public bool HasView(int dataIndex)
        {
            var entry = GetEntry(dataIndex);
            return entry.HasView;
        }

        public bool IsFirstDataHasView => HasView(0);

        public bool IsLastDataHasView => HasView(this.Count - 1);

        public bool HasAnyView => this.viewStartIndex != -1;

        public int ViewCount => this.viewCount;

        public void ForeachView(Action<RectTransform> handler)
        {
            if(this.ViewCount == 0)
            {
                return;
            }
            for(int i = this.viewStartIndex; i <= this.viewEndIndex; i++)
            {
                var entry = GetEntry(i);
                var view = entry.view;
                handler.Invoke(view);
            }
        }

        public RectTransform GetView(int dataIndex)
        {
            var entry = this.GetEntry(dataIndex);
            if (!entry.HasView)
            {
                throw new Exception($"[VerticalVirtualLayout] dataIndex {dataIndex} not visible");
            }
            
            return entry.view;
        }

        public RectTransform LastView
        {
            get
            {
                if (this.viewCount == 0)
                {
                    return null;
                }
                return GetView(this.viewEndIndex);
            }
        }

        public RectTransform FirstView
        {
            get
            {
                if (this.viewCount == 0)
                {
                    return null;
                }
                return GetView(this.viewStartIndex);
            }
        }

        public int FirstViewDataIndex => this.viewStartIndex;
        public int LastViewDataIndex => this.viewEndIndex;

        public bool IsDataIndexValid(int dataIndex)
        {
            if (dataIndex < 0)
            {
                return false;
            }
            if (dataIndex >= this.Count)
            {
                return false;
            }
            return true;
        }

        public int ViewToDataIndex(RectTransform view)
        {
            if(view == null)
            {
                throw new Exception("[VirtualVertical] argument can not be null");
            }
            for(int index = this.viewStartIndex; index <= this.viewEndIndex; index++)
            {
                var entry = this.GetEntry(index);
                if(entry.view == view)
                {
                    return index;
                }
            }
            return -1;
        }
    }

    public class DataEntry
    {
        public object data;
        public RectTransform view;

        [ShowInInspector]
        public bool HasView => view != null;
    }
}