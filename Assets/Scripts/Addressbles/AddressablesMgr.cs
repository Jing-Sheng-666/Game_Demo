using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesInfo
{
    public AsyncOperationHandle handle;
    public uint count;
    public AddressablesInfo(AsyncOperationHandle handle)
    {
        this.handle = handle;
        count += 1;
    }
}

public class AddressablesMgr : BaseManager<AddressablesMgr>
{
    //用于存储异步加载的返回值 类型AsyncOperationHandle<T>
    public Dictionary<string, AddressablesInfo> AddressablesDic = new Dictionary<string, AddressablesInfo>();
    private AddressablesMgr()
    {
    }
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    public void LoadAssetAsync<T>(string name, Action<AsyncOperationHandle<T>> callback)
    {
        //存在同名，对同名进行区分
        string keyName = name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        //如果存在同名的资源，直接返回
        if (AddressablesDic.ContainsKey(keyName))
        {
            handle = AddressablesDic[keyName].handle.Convert<T>();
            AddressablesDic[keyName].count += 1;
            //判断是否加载完成
            if (handle.IsDone)
            {
                callback?.Invoke(handle);//失败的逻辑下面已经处理了，逻辑到这里必定是成功的
            }
            else
            {
                //如果没有加载完成，添加回调
                handle.Completed += (obj =>
                {
                    OnAsyncLoadCompleted(keyName, obj, callback);
                });
            }

            return;//后面逻辑不走了
        }
        //不存在同名的资源，进行异步加载
        handle = Addressables.LoadAssetAsync<T>(name);
        handle.Completed += (obj =>
        {
            OnAsyncLoadCompleted(keyName, obj, callback);
        });
        AddressablesDic.Add(keyName, new AddressablesInfo(handle));
    }
    /// <summary>
    /// 异步加载完成的回调
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keyName"></param>
    /// <param name="handle"></param>
    /// <param name="callback"></param>
    public void OnAsyncLoadCompleted<T>(string keyName, AsyncOperationHandle<T> handle, Action<AsyncOperationHandle<T>> callback)
    {
        if(handle.Status == AsyncOperationStatus.Succeeded)
        {
            callback?.Invoke(handle);
        }
        else
        {
            Debug.LogWarning(keyName + "资源加载失败");

            //加载失败，移出字典
            //为了防止两次调用加载方法，需要加上判断，防止重复移除
            if (AddressablesDic.ContainsKey(keyName))
            {
                AddressablesDic.Remove(keyName);
            }
        }
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    public void ReleaseAsset<T>(string name)
    {
        string keyName = name + "_" + typeof(T).Name;
        if (AddressablesDic.ContainsKey(keyName))
        {
            AddressablesDic[keyName].count -= 1;
            if(AddressablesDic[keyName].count == 0)
            {
                //取出对象 释放资源 移出字典
                AsyncOperationHandle<T> handle = AddressablesDic[keyName].handle.Convert<T>();
                Addressables.Release(handle);
                AddressablesDic.Remove(keyName);
            }
        }
    }
    /// <summary>
    /// 清理所有资源
    /// </summary>
    public void ClearAsset()
    {
        foreach (var item in AddressablesDic)
        {
            Addressables.Release(item.Value.handle);   
        }
        AddressablesDic.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    /// <summary>
    /// 异步加载资源组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mergeMode"></param>
    /// <param name="callback"></param>
    /// <param name="keys"></param>
    public void LoadAssetsAsync<T>(Addressables.MergeMode mergeMode, Action<T> callback,params string[] keys)
    {
        //构建keyName方便后面存入字典
        List<string> list = new List<string>(keys);
        string keyName ="";
        foreach (var key in list)
        {
            keyName += key + "_";
        }
        keyName += typeof(T).Name;
        AsyncOperationHandle<IList<T>> handle;
        //判断是否存在已经加载的资源组
        if (AddressablesDic.ContainsKey(keyName))
        {
            handle = AddressablesDic[keyName].handle.Convert<IList<T>>();
            AddressablesDic[keyName].count += 1;
            //判断是否加载完成
            if (handle.IsDone)
            {
                foreach (var item in handle.Result)
                {
                    callback?.Invoke(item);
                }
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if(obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        foreach (var item in obj.Result)
                        {
                            callback?.Invoke(item);
                        }
                    }
                };
            }
            return;
        }
        //不存在同名的资源组，进行异步加载
        handle =Addressables.LoadAssetsAsync<T>(list, callback, mergeMode);
        handle.Completed += (obj) =>
        {
            //不处理成功逻辑是因为，成功的逻辑已经在callback中处理了
            if(obj.Status == AsyncOperationStatus.Failed)
            {
                if(AddressablesDic.ContainsKey(keyName))
                {
                    AddressablesDic.Remove(keyName);
                }
            }
        };
        AddressablesDic.Add(keyName, new AddressablesInfo(handle));
    }

    public void ReleaseAssets<T>(params string[] keys)
    {
        //构建keyName方便后面存入字典
        List<string> list = new List<string>(keys);
        string keyName ="";
        foreach (var key in list)
        {
            keyName += key + "_";
        }
        keyName += typeof(T).Name;

        if(AddressablesDic.ContainsKey(keyName))
        {
            AddressablesDic[keyName].count -= 1;
            if(AddressablesDic[keyName].count == 0)
            {
                AsyncOperationHandle<IList<T>> handle = AddressablesDic[keyName].handle.Convert<IList<T>>();
                Addressables.Release(handle);
                AddressablesDic.Remove(keyName);
            }
        }
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T LoadAssetSync<T>(string name)
    {
        string keyName = name + "_" + typeof(T).Name;
        if (AddressablesDic.ContainsKey(keyName) && AddressablesDic[keyName].handle.IsDone)
        {
            AddressablesDic[keyName].count += 1;
            return AddressablesDic[keyName].handle.Convert<T>().Result;
        }


        var handle = Addressables.LoadAssetAsync<T>(name);   // ① 先拿到 handle
        T result = handle.WaitForCompletion();               // ② 再等它完成，拿到结果
        AddressablesDic.Add(keyName, new AddressablesInfo(handle));
        return result;
    }


}
