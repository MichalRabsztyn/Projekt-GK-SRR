using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationId destinationPortal;
    [SerializeField] Transform spawnPoint;
    [SerializeField] SceneDetails sceneDetails;

    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
    }

    Fader fader;
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);

        GameController.Instance.PauseGame(true);

        yield return fader.FadeIn(0.4f);

        yield return SceneManager.LoadSceneAsync(sceneToLoad);

        var destinationPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);

        player.Character.SetPositionAndSnapToTile(destinationPortal.spawnPoint.position);

        GameController.Instance.SetCurrentSceneActive(SceneManager.GetActiveScene().buildIndex);

        yield return fader.FadeOut(0.4f);
        player.ShowOnScreenInfo();
        GameController.Instance.PauseGame(false);

        Destroy(gameObject);
    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationId
{ A, B, C, D};