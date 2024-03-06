using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class GameMapController : MonoBehaviour
{
    private Camera mainCamera;
    public MapUIManager mapUiManager;
    private MapTutorialController mapTutorialController;
    
    void OnEnable()
    {
        InputManager.OnPrimaryUpdated += TapLocation;
        LevelDetails.OnPlayLevel += LoadCampaign;
    }
    void OnDisable()
    {
        InputManager.OnPrimaryUpdated -= TapLocation;
        LevelDetails.OnPlayLevel -= LoadCampaign;
    }
    // Start is called before the first frame update
    void Awake()
    {
        mainCamera = Camera.main;
        mapTutorialController = GetComponent<MapTutorialController>();
    }

    void Update()
    {
        AudioManager.Instance.AudioUpdate();
    }

    void TapLocation()
    {
        if (mapUiManager.IsUiOpen()) return;
        if (!mapTutorialController.allowSelectingVisions) return;
        Vector2 pointerPos = InputManager.Instance.GetPointerPosition();
        Ray hitRay = mainCamera.ViewportPointToRay(pointerPos);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(hitRay, 20);

        if (hit2D)
        {
            MapLevel clickedLevel = hit2D.transform.GetComponent<MapLevel>();
            if (clickedLevel != null && clickedLevel.levelState != MapLevel.levelStatus.Locked)
            {
                if(mapTutorialController.showTutorial){ mapTutorialController.ClosePanel(1);}
                clickedLevel.SelectLevel();
            }
        }
    }

    void LoadCampaign(MapLevel levelToLoad)
    {
        SceneManager.Instance.LoadScene(SceneManager.Scenes.GameScene);
    }
}
