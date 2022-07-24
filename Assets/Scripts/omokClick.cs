using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.EventSystems;
using UnityEngine.UI;

public class omokClick : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    [Header("세로")]
    public int column;//세로 ↕
    
    [Header("가로")]
    public int row;//가로 ↔

    [Space(20)]
    [SerializeField] private Image icon;
    [SerializeField] private GameObject stoneHelopOb;
    [SerializeField] private GameObject noOb;

    public bool Check()
    {
        if (omokManager.inst.IsPlay == false)
        {
            return false;
        }

        if (omokManager.inst.IsBlackTurn)
        {
            if (omokManager.inst.myPlayType!=ePlayType.BLACK)
            {
            return false;
                
            }
        }
        else
        {
            if (omokManager.inst.myPlayType!=ePlayType.WHITE)
            {
                return false;
                
            }
        }
        
        if (omokManager.inst.ball[column,row]!=0)
        {
            return false ;
        }
        
        return true;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Check()==false)
        {
            return;
        }

        stoneHelopOb.gameObject.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Check()==false)
        {
            return;
        }
        stoneHelopOb.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Check()==false)
        {
            return;
        }
        
        
        stoneHelopOb.gameObject.SetActive(false);
        var color = transform.GetComponent<Image>().color;
        color.a = 1f;
        
        GetComponent<Image>().color = color;
        
        if (omokManager.inst.IsBlackTurn)
        {
            icon.sprite = omokManager.inst.dollImg[0];
        }
        else
        {
            icon.sprite = omokManager.inst.dollImg[1];
        }
        omokManager.inst.BallClick(row, column);
        
    }

    public void ballClick(int num)
    {
        if (num==1)
        {
            var color = transform.GetComponent<Image>().color;
            color.a = 1f;
            GetComponent<Image>().color = color;
            icon.sprite = omokManager.inst.dollImg[0];
            
        }
        else if (num == 2)
        {
            var color = transform.GetComponent<Image>().color;
            color.a = 1f;
            GetComponent<Image>().color = color;
            icon.sprite = omokManager.inst.dollImg[1];
        }
        else if (num == 3)
        {
            noOb.SetActive(true);
            omokManager.inst.noObs.Add(gameObject);
        }
    }


    public void NoOff()
    {
        omokManager.inst.ball[column, row] = 0;
        noOb.SetActive(false);
    }

    public void Clear()
    {
        var color = transform.GetComponent<Image>().color;
        color.a = 0f;
        GetComponent<Image>().color = color;
        omokManager.inst.ball[column, row] = 0;
        noOb.SetActive(false);
    }
    
    
    
    
    [ContextMenu ("Do Something")]
    void DoSomething ()
    {
        Transform a = GameObject.Find("Grid").transform;
        for (int i = 0; i <= 14; i++)
        {
            for (int j = 0; j <= 14; j++)
            {
                GameObject ob = Instantiate(gameObject, a);
                ob.name = $"{i},{j}";
                
            }
        }
    }
    [ContextMenu ("name")]
    void nameset ()
    {
        string[] s = gameObject.name.Split(',');
        row = int.Parse(s[1]) ;
        column = int.Parse(s[0]);
    }


}
