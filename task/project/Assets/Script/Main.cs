using System.Collections;
using UnityEngine;

public class Main : MonoSingleton<Main>
{
    private IEnumerator Start()
    {
        var uiMain = UIManager.Instance.GetUI<UIMain>("pf_ui_main").On();
        var patch = new IPatch[]
        {
            DataManager.Instance,
        };

        foreach (var p in patch)
        {
            yield return p.Patch(null);
        }

        uiMain.Off();

        ModeManager.Instance.Enter(SagaGameData.Instance.lobbyModeID);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        
        ObjectManager.Instance.UpdateDt(dt);
        ModeManager.Instance.UpdateDt(dt);
    }
}
