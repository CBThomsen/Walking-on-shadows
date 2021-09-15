using UnityEngine;
using Zenject;

public class CoreInstaller : MonoInstaller
{
    public LightSystem lightSystem;
    public ShadowSystem shadowSystem;

    public override void InstallBindings()
    {
        //Add level objects
        Container.Bind<SceneGeometry>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesAndSelfTo<ComputeBuffers>().AsSingle();
        Container.Bind<LightSystem>().FromInstance(lightSystem);
        Container.Bind<ShadowSystem>().FromInstance(shadowSystem);

    }
}