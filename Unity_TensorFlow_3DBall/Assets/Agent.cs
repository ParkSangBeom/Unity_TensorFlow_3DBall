using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.Text;
using System;

public class Data
{
    public List<float> Obs = new List<float>();
    public int Action = 0;
    public float Reward = 0.0f;
}

public enum State
{
    None = 0,
    Start,
    Loading,
    Update,
}

public class Agent : MonoBehaviour
{
    public GameObject _planeObject = null;
    public GameObject _sphereObject = null;
    private Rigidbody _sphereRigidbody = null;

    private List<Data> _lsData = new List<Data>();

    private Client _client = null;

    private float _timeScale = 1.0f;
    private int _globalCnt = 0;
    private int _step = 0;

    private State _state = State.None;
    private Data _curData = new Data();


    public void SetClient(Client client)
    {
        _sphereRigidbody = _sphereObject.GetComponent<Rigidbody>();
        _client = client;
    }

    public void AgentStart()
    {    
        _state = State.Start;
    }

    Vector3 preVelocity = Vector3.zero;
    Vector3 prePos = Vector3.zero;
    Quaternion prePlaneAngle = Quaternion.identity;

    private void Update()
    {
        if (_state == State.Loading)
        {
            Time.timeScale = 0.0f;
            return;
        }
        else if(_state == State.Start)
        {
            AgentReset();
            preVelocity = Vector3.zero;
            prePos = Vector3.zero;
            prePlaneAngle = Quaternion.identity;

            _curData = new Data();
            _lsData.Add(_curData);

            List<float> obs = GetObservations();
            _curData.Obs = obs;

            Time.timeScale = _timeScale;
            _state = State.Loading;
            string actionJsonData = GetJsonAction(obs);
            _client.SendData(actionJsonData);
        }
        else if(_state == State.Update)
        {          
            Time.timeScale = _timeScale;
            Step(_curData.Action);
            //float d1 = prePos.x - _planeObject.transform.position.x;
            //float d2 = _sphereRigidbody.angularVelocity.z - preVelocity.z;
            //float reward1 = d1 * d2 > 0 && Mathf.Abs(_sphereRigidbody.angularVelocity.z) < 1.5f ? 1.0f : -1.0f;

            //float d3 = prePos.z - _planeObject.transform.position.z;
            //float d4 = _sphereRigidbody.angularVelocity.x - preVelocity.x;
            //float reward2 = d3 * d4 < 0 && Mathf.Abs(_sphereRigidbody.angularVelocity.x) < 1.5f ? 1.0f : -1.0f;

            //float reward = (reward1 > 0 && reward2 > 0) ? 1.0f : -1.0f;
            //_curData.Reward = reward1;

            //if (_sphereObject.transform.position.x > 0.0f)
            //{
            //    if (_curData.Action == 0 || _curData.Action == 2)
            //    {
            //        _curData.Reward = 1.0f;
            //    }
            //    else
            //    {
            //        _curData.Reward = -1.0f;
            //    }
            //}
            //else
            //{
            //    if (_curData.Action == 1 || _curData.Action == 3)
            //    {
            //        _curData.Reward = 1.0f;
            //    }
            //    else
            //    {
            //        _curData.Reward = -1.0f;
            //    }
            //}

            if (IsDone())
            {
                _curData.Reward = -10.0f;
                _state = State.Loading;

                string trainJsonData = GetTrainJsonData();
                _client.SendData(trainJsonData);
                return;
            }

            _curData.Reward = 0.01f;
            preVelocity = _sphereRigidbody.angularVelocity;
            prePos = _sphereObject.transform.position;
            prePlaneAngle = _planeObject.transform.rotation;

            _curData = new Data();
            _lsData.Add(_curData);

            List<float> obs = GetObservations();
            _curData.Obs = obs;

            _step++;
            _state = State.Loading;
            string actionJsonData = GetJsonAction(obs);
            _client.SendData(actionJsonData);
        }      
    }

    private List<float> GetObservations()
    {
        List<float> obs = new List<float>();

        obs.Add(_sphereObject.transform.position.x);
        //obs.Add(_sphereObject.transform.position.y);
        obs.Add(_sphereObject.transform.position.z);
        obs.Add(_sphereRigidbody.angularVelocity.x);
        //obs.Add(_sphereRigidbody.angularVelocity.y);
        obs.Add(_sphereRigidbody.angularVelocity.z);

        //obs.Add(_planeObject.transform.rotation.x);
        //obs.Add(_planeObject.transform.rotation.y);
        //obs.Add(_planeObject.transform.rotation.z);

        return obs;
    }

    private void Step(int action)
    {
        float x = 0.0f;
        float z = 0.0f;
        float rotation_value = 1.0f;

        switch (action)
        {
            case 0:
                x = rotation_value;
                z = rotation_value;
                break;

            case 1:
                x = rotation_value;
                z = 0;
                break;

            case 2:
                x = rotation_value;
                z = -rotation_value;
                break;

            case 3:
                x = 0;
                z = rotation_value;
                break;

            case 4:
                x = 0;
                z = 0;
                break;

            case 5:
                x = 0;
                z = -rotation_value;
                break;

            case 6:
                x = -rotation_value;
                z = rotation_value;
                break;

            case 7:
                x = -rotation_value;
                z = 0;
                break;

            case 8:
                x = -rotation_value;
                z = -rotation_value;
                break;

                //case 0:
                //    z = 1.0f;
                //    break;

                //case 1:
                //    z = -1.0f;
                //    break;

                //case 2:
                //    z = 0.5f;
                //    break;

                //case 3:
                //    z = -0.5f;
                //break;

                //case 0:
                //    x = rotation_value;
                //    break;

                //case 1:
                //    x = -rotation_value;
                //    break;
        }
        //z = Mathf.Clamp(z, -1.0f, 1.0f);
        if ((_planeObject.transform.rotation.z < 0.10f && z > 0f) ||
            (_planeObject.transform.rotation.z > -0.10f && z < 0f))
        {
            _planeObject.transform.Rotate(new Vector3(0, 0, 1), z);
        }

        if ((_planeObject.transform.rotation.x < 0.10f && x > 0f) ||
            (_planeObject.transform.rotation.x > -0.10f && x < 0f))
        {
            _planeObject.transform.Rotate(new Vector3(1, 0, 0), x);
        }
    }

    private bool IsDone()
    {
        Vector3 spherePos = _sphereObject.transform.position;
        Vector3 planeRot = _planeObject.transform.eulerAngles;
        //float vel = Mathf.Abs(_sphereRigidbody.angularVelocity.x) + Mathf.Abs(_sphereRigidbody.angularVelocity.y) + Mathf.Abs(_sphereRigidbody.angularVelocity.z);
        if (Mathf.Abs(spherePos.x) > 1.0f || Mathf.Abs(spherePos.z) > 1.0f)
        {
            return true;
        }

        return false;
    }

    private string GetJsonAction(List<float> obs)
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter jsonw = new JsonWriter(sb);
        jsonw.WriteObjectStart();
        {
            jsonw.WritePropertyName("type");
            jsonw.Write("action");

            jsonw.WritePropertyName("datalist");
            jsonw.WriteArrayStart();
            {
                jsonw.WriteObjectStart();
                {
                    for (int i = 0; i < obs.Count; ++i)
                    {
                        jsonw.WritePropertyName("v" + i.ToString());
                        jsonw.Write(obs[i]);
                    }
                }
                jsonw.WriteObjectEnd();
            }
            jsonw.WriteArrayEnd();
        }
        jsonw.WriteObjectEnd();

        return sb.ToString();
    }

    private string GetTrainJsonData()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter jsonw = new JsonWriter(sb);
        jsonw.WriteObjectStart();
        {
            jsonw.WritePropertyName("type");
            jsonw.Write("train");

            jsonw.WritePropertyName("datalist");
            jsonw.WriteArrayStart();
            {
                for (int i = 0; i < _lsData.Count; ++i)
                {
                    jsonw.WriteObjectStart();
                    {
                        List<float> obs = _lsData[i].Obs;
                        for (int k = 0; k < obs.Count; ++k)
                        {
                            jsonw.WritePropertyName("v" + k.ToString());
                            jsonw.Write(obs[k]);
                        }

                        jsonw.WritePropertyName("action");
                        jsonw.Write(_lsData[i].Action);

                        jsonw.WritePropertyName("reward");
                        jsonw.Write(_lsData[i].Reward * _lsData.Count);
                    }
                    jsonw.WriteObjectEnd();
                }
            }
            jsonw.WriteArrayEnd();
        }
        jsonw.WriteObjectEnd();

        return sb.ToString();
    }

    public void LitsenJsonData(string data)
    {
        JsonData jsonData = JsonMapper.ToObject(data);
        string result = jsonData["result"].ToString();
        if (result == "train_true")
        {
            AgentStart();
        }
        else if (result == "action_true")
        {
            _curData.Action = int.Parse(jsonData["value"].ToString());
            _state = State.Update;
        }
    }

    private void AgentReset()
    {
        _sphereRigidbody.velocity = Vector3.zero;
        _sphereRigidbody.angularVelocity = Vector3.zero;
        _sphereObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        _sphereObject.transform.position = new Vector3(0.0f, 0.5f, 0.0f);

        _planeObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        _planeObject.transform.Rotate(new Vector3(1, 0, 0), UnityEngine.Random.Range(-10f, 10f));
        _planeObject.transform.Rotate(new Vector3(0, 0, 1), UnityEngine.Random.Range(-10f, 10f));

        _lsData.Clear();

        print("Step : " + _step);
        _step = 0;
        print("E : " + _globalCnt);
        _globalCnt += 1;
    }

    private float GetEulerAngles(float angle)
    {
        if (angle < 0)
        {
            angle += 360.0f;
        }

        return angle;
    }
}
