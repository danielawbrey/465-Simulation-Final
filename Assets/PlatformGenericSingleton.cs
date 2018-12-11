// -----------------------------------------------------------------------
// Project: MACHINE											   System: N/A
// -----------------------------------------------------------------------
// File = PlatformGenericSinglton.cs
//
// ENVIRONMENT/LANGUAGE:
// Unity 2017.3.1p1 / C#
//
// (c) Copyright 2018 Noorcon Inc.
//     Published Work, All Rights Reserved
//
// =======================================================================
// OVERVIEW:
//
// Generic Singlton that can be inherited to create true singlton for any
// class.
//
// =======================================================================
// VERSION HISTORY:
//
// Version 1.0 - New Release, 03/29/2018
//
// =======================================================================
// AUTHOR: Vahe Karamian
// E-MAIL: vahe.karamian@noorcon.com
//
// =======================================================================

using UnityEngine;

public class PlatformGenericSingleton<T> : MonoBehaviour where T : Component {
    private static T instance;

    public static T Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<T>();
                if (instance == null) {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    public virtual void Awake() {
        if (instance == null) {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
