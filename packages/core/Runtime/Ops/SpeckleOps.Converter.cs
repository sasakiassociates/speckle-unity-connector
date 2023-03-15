using Cysharp.Threading.Tasks;
using Speckle.ConnectorUnity.Converter;
using Speckle.ConnectorUnity.Models;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Speckle.ConnectorUnity.Ops
{

  public static partial class SpeckleOps
  {

    public static async UniTask<SpeckleObjectHierarchy> ConvertToNative(ISpeckleConverter converter, Base data, Transform parent, CancellationToken token)
    {

      if(data == null)
      {
        SpeckleUnity.Console.Warn($"No valid {typeof(Base)} to use during conversion ");
        return null;
      }

      if(parent == null)
      {
        SpeckleUnity.Console.Warn("No valid Parent to use during conversion ");
        return null;
      }

      if(converter == null)
      {
        SpeckleUnity.Console.Warn("No valid converter to use during conversion ");
        return null;
      }

      SpeckleUnity.Console.Log("Starting To Convert");

      var hierarchy = new SpeckleObjectHierarchy(parent);

      var layer = hierarchy.parent.GetComponent<SpeckleLayer>();

      if(layer == null)
      {
        layer = hierarchy.parent.gameObject.AddComponent<SpeckleLayer>();
      }

      layer.Parent = hierarchy.parent;

      hierarchy.SetDefault(layer);
      const int BATCH_SIZE = 500;

      SpeckleUnity.Console.Log("Traversing through objects");

      await TraverseObject(hierarchy.DefaultLayer, data, converter, token, BATCH_SIZE, 0);

      SpeckleUnity.Console.Log("Traverse Complete");

      if(hierarchy.DefaultLayer.Layers.Any())
      {
        hierarchy.DefaultLayer.ParentObjects(hierarchy.parent);
      }
      else
      {
        Utils.SafeDestroy(hierarchy.DefaultLayer);
        SpeckleUnity.Console.Warn("No valid data was found during conversion");
      }

      SpeckleUnity.Console.Log("Starting post work");


      if(converter is ScriptableConverter sc)
      {
        SpeckleUnity.Console.Log("Starting post work");
        await sc.PostWork();
        SpeckleUnity.Console.Log("Post work complete");
      }

      if(converter.Report != null && converter.Report.ConversionErrors.Any())
      {
        foreach(var errors in converter.Report.ConversionErrors)
        {
          SpeckleUnity.Console.Warn(errors.Message);
        }
      }
      return hierarchy;
    }

    static async UniTask TraverseObject(SpeckleLayer layer, Base @base, ISpeckleConverter converter, CancellationToken token, int batchSize, int batchIndex)
    {
      if(token.IsCancellationRequested) return;

      if(batchIndex >= batchSize)
      {
        await UniTask.Yield();
        batchIndex = 0;
      }

      // 1: Object is supported, so lets convert the object and it's data 
      if(converter.CanConvertToNative(@base))
      {
        var obj = CheckConvertedFormat(converter.ConvertToNative(@base));
        batchIndex++;

        if(obj != null)
        {
          layer.Add(obj);
        }
      }

      // 2: Check for the properties of an object. There might be additional objects that we want to convert as well
      foreach(var member in @base.GetDynamicMembers())
      {
        if(token.IsCancellationRequested) return;

        var propObject = @base[member];

        if(propObject.IsBase(out var probBase))
        {
          // 2a: Member is a speckle object
          await TraverseObject(layer, probBase, converter, token, batchSize, batchIndex);
        }
        else if(propObject.IsList(out var nestedObjects))
        {
          // 2b: A prop is a list, we create a special list object to store those items in
          var nestedLayer = new GameObject(member).AddComponent<SpeckleLayer>();
          nestedLayer.transform.SetParent(layer.transform);
          batchIndex++;

          foreach(var nestedObj in nestedObjects)
          {
            if(token.IsCancellationRequested) return;

            if(nestedObj.IsBase(out var nestedBase))
            {
              await TraverseObject(nestedLayer, nestedBase, converter, token, batchSize, batchIndex);
            }
          }

          nestedLayer.ParentObjects();
          layer.Add(nestedLayer);
        }
        else
        {
          converter.Report.LogConversionError(new Exception($"Unhandled type {@base.speckle_type} was not converted"));
        }
      }
    }

  }

}
