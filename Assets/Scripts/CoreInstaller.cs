using UnityEngine;
using Zenject;

public class CoreInstaller : MonoInstaller
{
    public static string levelName;

    public LightSystem lightSystem;
    public ShadowSystem shadowSystem;

    public override void InstallBindings()
    {
        //Add level objects
        Container.Bind<SceneGeometry>().FromComponentInNewPrefabResource("Levels/" + levelName).AsSingle();

        Container.BindInterfacesAndSelfTo<ComputeBuffers>().AsSingle();
        Container.Bind<LightSystem>().FromInstance(lightSystem);
        Container.Bind<ShadowSystem>().FromInstance(shadowSystem);

    }
}