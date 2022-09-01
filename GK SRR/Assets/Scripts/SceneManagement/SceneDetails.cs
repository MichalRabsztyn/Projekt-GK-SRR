using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] public AudioClip audioClip;
    [SerializeField] string SceneName;

    public static SceneDetails i { get; private set; }

    private void Awake()
    {
        i = this;
        SceneName = SceneManager.GetActiveScene().name;
    }

}
