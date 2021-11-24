using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI; 

public partial class DashboardPage : Page
{

    public static Action okHandler;

    List<Type> _modelTypeList;
    public List<Type> ModelTypeList
    {
        get
        {
            if(_modelTypeList == null)
            {
                _modelTypeList = ReflectionUtil.GetAttributedClassesInAllAssemblies<DashboardSettingsAttribute>();
            }
            return _modelTypeList;
        }
    }


    public Dictionary<PropertyInfo, List<string>> propertyToOptionListDic = new Dictionary<PropertyInfo, List<string>>();
    public List<PropertyInfo> labelPropertyList = new List<PropertyInfo>();
    public List<PropertyInfo> checkboxPropertyList = new List<PropertyInfo>();

    void FetchOptionsMenmber()
    {
        propertyToOptionListDic.Clear();
        labelPropertyList.Clear();
        checkboxPropertyList.Clear();

        var typeList = this.ModelTypeList;
        foreach(var type in typeList)
        {
            var propertyList = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var property in propertyList)
            {
                var attribute = property.GetCustomAttribute<DashboardGuiAttribute>();
                if (attribute != null)
                {
                    var guiType = attribute.guiType;
                    if (guiType == DashboardGuiType.Options)
                    {
                        var propertyName = property.Name;
                        var optionListProviderName = $"{propertyName}_GetOptionList";
                        var optionListProviderMethodInfo = type.GetMethod(optionListProviderName);
                        if (optionListProviderMethodInfo == null)
                        {
                            throw new Exception($"[DashbaordPage] method: {optionListProviderName} not found");
                        }
                        var optionList = optionListProviderMethodInfo.Invoke(null, new object[] { });
                        var optionListAsString = optionList as List<string>;
                        if (optionListAsString == null)
                        {
                            throw new Exception($"[DashbaordPage] return value of method: {optionListProviderName}, which can not cast to List<string> type.");
                        }
                        propertyToOptionListDic[property] = optionListAsString;
                    }
                    else if (guiType == DashboardGuiType.Label)
                    {
                        labelPropertyList.Add(property);
                    }
                    else if(guiType == DashboardGuiType.Checkbox)
                    {
                        var valueType = property.PropertyType;
                        if (valueType == typeof(bool))
                        {
                            checkboxPropertyList.Add(property);
                        }
                        else
                        {
                            Debug.LogError($"[DashboardPage] {type}.{property.Name} marked Checkbox, but value is not bool");
                        }
                    }
                }
            }
        }
        
    }

    void Awake()
    {
        this.FetchOptionsMenmber();
    }

    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        this.Refresh();
    }

    void Refresh()
    {
        this.RefreshItemList();
    }

    void RefreshItemList()
    {
        TransformUtil.RemoveAllChildren(this.ItemRoot.transform);
        this.Prefab_dashboardItemView.gameObject.SetActive(false);

        this.CreateAllDropdownView();
        this.CreateAllLabelView();
        this.CreateAllCheckBoxView();
        
    }

    void CreateAllCheckBoxView()
    {
        foreach (var property in checkboxPropertyList)
        {
            var name = property.Name;
            var value = (bool)property.GetValue(null);
            var checkboxGroup = CreateView<DashboardItemCheckboxGroup>(DashboardItemType.Checkbox);

            checkboxGroup.headLabel.text = name;
            checkboxGroup.toggle.isOn = value;

            var toggle = checkboxGroup.toggle;
            toggle.isOn = value;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(newValue =>
            {
                property.SetValue(null, newValue);
            });
        }
    }

    T CreateView<T>(DashboardItemType type) where T : MonoBehaviour
    {
        var view = GameObject.Instantiate<DashboardItemView>(this.Prefab_dashboardItemView, this.ItemRoot.transform);
        view.ViewType = type;
        var group = view.ActivedGroup;
        var groupComp = group.GetComponent<T>();
        view.gameObject.SetActive(true);
        return groupComp;
    }


    void CreateAllLabelView()
    {

        foreach (var property in labelPropertyList)
        {
            var name = property.Name;
            var value = property.GetValue(null);

            var labelGroup = CreateView<DashboardItemLabelGroup>(DashboardItemType.Label);
            var text_head = labelGroup.headLabel;
            var text_value = labelGroup.valueLabel;

            text_head.text = name + ":";
            text_value.text = value.ToString();
        }
    }

    void CreateAllDropdownView()
    {
        foreach (var kv in propertyToOptionListDic)
        {
            var property = kv.Key;
            var optionList = kv.Value;
            var dropdownGroup = CreateView<DashboardItemDropDownGroup>(DashboardItemType.DropDown);
            var dropdown = dropdownGroup.dropdown;
            var text_headlabel = dropdownGroup.headlabel;

            var optionDataList = CreateDropDownOptionList(optionList);
            dropdown.options = optionDataList;

            var propertyName = property.Name;
            text_headlabel.text = propertyName + ":";

            var currentValue = property.GetValue(null) as string;

            var currentIndex = optionList.IndexOf(currentValue);
            dropdown.value = currentIndex;

            dropdown.onValueChanged.AddListener((index) =>
            {
                var newValue = optionList[index];
                property.SetValue(null, newValue);
            });
            dropdown.gameObject.SetActive(true);
        }
    }

    List<Dropdown.OptionData> CreateDropDownOptionList(List<string> stringList)
    {
        var list = new List<Dropdown.OptionData>();
        foreach(var one in stringList)
        {
            var data = new Dropdown.OptionData(one);
            list.Add(data);
        }
        return list;
    }

    public void OnButton(string msg)
    {
        if(msg == "ok")
        {
            if (okHandler == null)
            {
                throw new Exception("[DashboardPage] okHandler not set yet");
            }
            okHandler.Invoke();
        }
    }
}
