using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

public static class NetUtils
{
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static void Put( this NetDataWriter netDataWriter, Vector3 value ){
        netDataWriter.Put(value.x);
        netDataWriter.Put(value.y);
        netDataWriter.Put(value.z);
    }
    public static void Put( this NetDataWriter netDataWriter, Color value) {
        netDataWriter.Put(value.r);
        netDataWriter.Put(value.g);
        netDataWriter.Put(value.b);
        netDataWriter.Put(value.a);
    }
    public static void PutArray( this NetDataWriter netDataWriter, Vector3[] value) {
        netDataWriter.Put(value.Length);
        for( int i = 0; i < value.Length; i++) {
            netDataWriter.Put(value[i]);
        }
    }
    public static void PutArray(this NetDataWriter netDataWriter, Color[] value) {
        netDataWriter.Put(value.Length);
        for (int i = 0; i < value.Length; i++) {
            netDataWriter.Put(value[i]);
        }
    }
    public static void PutArray(this NetDataWriter netDataWriter, List<NetworkedObject> objects) {
        int[] ids = new int[objects.Count];
        for (int i = 0; i < objects.Count; i++) {
            ids[i] = objects[i].id;
        }
        netDataWriter.PutArray(ids);
    }
    public static Vector3 GetVector3( this NetDataReader netDataReader ){
        float x = netDataReader.GetFloat();
        float y = netDataReader.GetFloat();
        float z = netDataReader.GetFloat();
        return new Vector3(x,y,z);
    }
    public static Color GetColor(this NetDataReader netDataReader) {
        float r = netDataReader.GetFloat();
        float g = netDataReader.GetFloat();
        float b = netDataReader.GetFloat();
        float a = netDataReader.GetFloat();
        return new Color(r, g, b, a);
    }
    public static Vector3[] GetVector3Array(this NetDataReader netDataReader) {
        Vector3[] arr = new Vector3[netDataReader.GetInt()];
        for( int i = 0; i < arr.Length; i++) {
            arr[i] = netDataReader.GetVector3();
        }
        return arr;
    }
    public static Color[] GetColorArray(this NetDataReader netDataReader) {
        Color[] arr = new Color[netDataReader.GetInt()];
        for (int i = 0; i < arr.Length; i++) {
            arr[i] = netDataReader.GetColor();
        }
        return arr;
    }
    public static List<NetworkedObject> GetNetworkedObjectArray(this NetDataReader netDataReader) {
        List<NetworkedObject> objects = new List<NetworkedObject>();
        int[] ids = netDataReader.GetIntArray();
        foreach( int id in ids) {
            if( NetworkedObject.objDict.ContainsKey(id)) {
                objects.Add(NetworkedObject.objDict[id]);
            }
        }
        return objects;
    }
}

// Solution by villevli from answers.unity.com
// https://answers.unity.com/questions/1238142/version-of-transformtransformpoint-which-is-unaffe.html
public static class TransformExtensions {
    public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position) {
        var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        return localToWorldMatrix.MultiplyPoint3x4(position);
    }

    public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position) {
        var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
        return worldToLocalMatrix.MultiplyPoint3x4(position);
    }
}