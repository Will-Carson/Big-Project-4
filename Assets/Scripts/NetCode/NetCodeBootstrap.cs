using Unity.NetCode;

// The preserve attibute is required to make sure the bootstrap is not stripped in il2cpp builds with stripping enabled
[UnityEngine.Scripting.Preserve]
// The bootstrap needs to extend `ClientServerBootstrap`, there can only be one class extending it in the project
public class NetCodeBootstrap : ClientServerBootstrap
{
    // The initialize method is what entities calls to create the default worlds
    public override bool Initialize(string defaultWorldName)
    {
#if UNITY_EDITOR
        // If we are in the editor, we check if the loaded scene is "Frontend",
        // if we are in a player we assume it is in the frontend if FRONTEND_PLAYER_BUILD
        // is set, otherwise we assume it is a single level.
        // The define FRONTEND_PLAYER_BUILD needs to be set in the build config for the frontend player.
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isFrontend = sceneName == "Frontend";
#elif !FRONTEND_PLAYER_BUILD
        bool isFrontend = false;
#endif

#if UNITY_EDITOR || !FRONTEND_PLAYER_BUILD
        if (!isFrontend)
        {
            AutoConnectPort = 7979;
            CreateDefaultClientServerWorlds();
        }
        else
        {
            AutoConnectPort = 0;
            CreateLocalWorld(defaultWorldName);
        }
#else
            CreateLocalWorld(defaultWorldName);
#endif
        return true;
    }
}
