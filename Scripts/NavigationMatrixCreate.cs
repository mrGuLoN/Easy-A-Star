using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationMatrixCreate : MonoBehaviour
{
    [SerializeField] private GameObject[] naviPoint, fireZone;
    public  int h, l;
    private GameObject[,] sortedNaviTransform;

    private void Awake()
    {
       sortedNaviTransform = new GameObject[h, l];
       int i = 0;
       for (int j = 0; j < h; j++)
       {
           for (int k = 0; k < l; k++)
           {
                sortedNaviTransform[j, k] = naviPoint[i];               
                i++;               
           }
       }
       
    }

    public void GiveNaviPoint(out GameObject[,] Navipoint, out GameObject[] Firepoint)
    {
        Navipoint = sortedNaviTransform;
        Firepoint = fireZone;
    }

}
