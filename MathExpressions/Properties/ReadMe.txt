
We are unable to share Properties\Resources.resx in other projects
because the ResXFileCodeGenerator creates the namespace based on
the path, and it includes all of the periods.

namespace MathExpressions.......Shared.Source.MathExpressions.Properties {

So, we have to copy the Resources.resx file to any project that needs it.


https://developercommunity.visualstudio.com/t/Using-a-Resourcesresx-file-from-another/10591549

  <ItemGroup>
    <EmbeddedResource Update="Properties\Resources.resx">
      <Generator>ResXFileCodeGenerator</Generator>
      <LastGenOutput>Resources.Designer.cs</LastGenOutput>
      <CustomToolNamespace>MathExpressions.Properties</CustomToolNamespace>
    </EmbeddedResource>
  </ItemGroup>
  <ItemGroup>
    <Compile Update="Properties\Resources.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>Resources.resx</DependentUpon>
    </Compile>
  </ItemGroup>

We could set a custom namespace, but the generated file still uses the project name in one place.

  [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
  internal static global::System.Resources.ResourceManager ResourceManager {
      get {
          if (object.ReferenceEquals(resourceMan, null)) {
              global::System.Resources.ResourceManager temp =
              		new global::System.Resources.ResourceManager("MathExpressions.MAUI.Properties.Resources", typeof(Resources).Assembly);
              resourceMan = temp;
          }
          return resourceMan;
      }
  }
