using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  partial class SkinGetPage : Page
{
    public void OnPreNavigatedTo(PageNavigateInfo info)
    {
        this.fragmentManager.OnPageNavigatedTo(info);
    }

    public void OnGet()
    {
        
    }
}
