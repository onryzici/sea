using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalvageCrew;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Runs in real advancing Play mode. UI events are programmatic, not native mouse input.
public static class UIPlayChecks
{
    public static async Task<object> Run()
    {
        if (!Application.isPlaying) throw new Exception("Play required");
        var results=new Dictionary<string,object>();
        Action<string,bool> check=(name,ok)=>{results[name]=ok;if(!ok)throw new Exception(name);};
        var session=HarborSession.Instance;
        var canvas=GameObject.Find("HarborNetworkSession/ConnectionHUD").transform;
        var panel=canvas.Find("ConnectionPanel");
        session.SetPanelOpen(true);await Task.Delay(250);
        check("panel_free_cursor",Cursor.lockState==CursorLockMode.None && session.PanelOpen);
        check("single_event_system",UnityEngine.Object.FindObjectsByType<EventSystem>().Length==1);
        check("panel_hides_reticle",!GameObject.Find("HarborPrototypeGenerated/HarborHUD/Crosshair").GetComponent<UnityEngine.UI.Image>().enabled);
        foreach(var name in new[]{"Host","Client","Disconnect","Resume","Address","Port"})
        {
            var target=(RectTransform)panel.Find(name);
            var hits=new List<RaycastResult>();
            var point=RectTransformUtility.WorldToScreenPoint(null,target.TransformPoint(target.rect.center));
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
            check("raycast_"+name,hits.Count>0&&(hits[0].gameObject.transform==target||hits[0].gameObject.transform.IsChildOf(target)));
        }
        session.ConfigureEndpoint("invalid",7777);
        panel.Find("Client").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();await Task.Delay(200);
        check("invalid_endpoint_feedback",session.Status.Contains("IPv4")&&!session.Manager.IsListening);
        session.ConfigureEndpoint("127.0.0.1",7777);
        panel.Find("Resume").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();await Task.Delay(200);
        check("resume_closes_panel",!session.PanelOpen);
        check("controls_restored",GameObject.Find("HarborPrototypeGenerated/HarborHUD/ControlsPanel").GetComponent<CanvasGroup>().alpha>.9f);
        session.SetPanelOpen(true);
        panel.Find("Host").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();await Task.Delay(1200);
        check("host_button_starts_session",session.Manager.IsHost&&session.LocalPlayer!=null);
        check("host_role_label",canvas.Find("Role").GetComponent<TextMeshProUGUI>().text.Contains("HOST"));
        check("single_network_hud",UnityEngine.Object.FindObjectsByType<NetworkCarryHud>().Length==1);
        var hud=UnityEngine.Object.FindAnyObjectByType<NetworkCarryHud>();
        check("network_hud_presentation",hud.GetComponent<HarborHudPresentation>()!=null);
        check("host_button_disabled",!panel.Find("Host").GetComponent<UnityEngine.UI.Button>().interactable);
        check("background_simulation",Application.runInBackground);
        panel.Find("Disconnect").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();await Task.Delay(1400);
        check("disconnect_restores_solo",!session.Manager.IsListening&&session.PanelOpen&&canvas.Find("Role").GetComponent<TextMeshProUGUI>().text.Contains("SOLO"));
        check("no_network_hud_left",UnityEngine.Object.FindObjectsByType<NetworkCarryHud>().Length==0);
        check("one_offline_hud",UnityEngine.Object.FindObjectsByType<CarryHud>().Length==1);
        var output=new {method="Programmatic UI raycast + onClick in real Editor Play. Not native OS clicks or a second client.",results};
        System.IO.File.WriteAllText("Docs/Verification/UIPlayResults.json",Newtonsoft.Json.JsonConvert.SerializeObject(output,Newtonsoft.Json.Formatting.Indented));
        return output;
    }
}
