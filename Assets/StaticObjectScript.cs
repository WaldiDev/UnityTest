using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class StaticObjectScript : MonoBehaviour
{

    public static int seed = -1;

    private static StaticObjectScript instance = null;
    public static StaticObjectScript Instance
    {
        get { return instance; }
    }

    // Use this for initialization
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }
}
