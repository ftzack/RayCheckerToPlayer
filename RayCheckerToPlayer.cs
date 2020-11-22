using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;
using DG.Tweening;
using Cinemachine;

public class RayCheckerToPlayer : MonoBehaviour
{
    public bool inTarget { get; private set; }

    //変数宣言
    #region
    [SerializeField] private GameObject targetObj = null;
    [SerializeField] private LayerMask layerMask = 0;
    [SerializeField] private float rayDistance = 1f;
    [SerializeField] private float addPositionX = 0.0f;
    [SerializeField] private float addPositionY = 0.0f;
    [SerializeField] private List<string> targetTags = null;

    [SerializeField] private bool setRayScopeArea = false;
    [SerializeField] private bool rayFrontOnly = false;
    [SerializeField] private bool showRayScope = false;
    [SerializeField] private bool ActiveUpdate = false;

    [SerializeField] private float maxDegrees = 0.0f;
    [SerializeField] private float minDegrees = 0.0f;


    private float rightMaxDegress;
    private float rightMinDegress;

    private RaycastHit2D hit;

    private Vector2 position;
    private Vector3 rightPositionDifference;
    private Vector3 leftPositionDifference;
    #endregion

    private Ray ray;
    private float deg;
    private bool playerIndeg = true;

    private void Reset()
    {
        rayDistance = 1.5f;
    }

    private void Start()
    {
        rightPositionDifference = new Vector3(addPositionX, addPositionY, 0f);
        leftPositionDifference = new Vector3(-addPositionX, addPositionY, 0f);
        rightMaxDegress = 180f - maxDegrees;
        rightMinDegress = -180f - minDegrees;
        StartCoroutine("RayUpdate");
    }

    private Vector2 EditingVector2(Vector2 targetVector)
    {

        //度の角度に変換
        deg = VectorToDeg(targetVector);

        //下の範囲外の判定
        if (deg < minDegrees && deg > rightMinDegress)
        {
            playerIndeg = false;
            //右によっていたら
            if (deg > -90f)
            {
                deg = minDegrees;
            }
            //左によっていたら
            else
            {
                deg = rightMinDegress;
            }
        }
        //上の範囲外の判定
        else if (deg > maxDegrees && deg < rightMaxDegress)
        {
            playerIndeg = false;
            //左によっていたら
            if (deg > 90f)
            {
                deg = rightMaxDegress;
            }
            //右によっていたら
            else
            {
                deg = maxDegrees;
            }
        }
        else
        {
            playerIndeg = true;
        }

        return DegToVector(deg);
    }
    private Vector2 EditingVector2FrontOnly(Vector2 targetVector)
    {

        //度の角度に変換
        deg = VectorToDeg(targetVector);

        if (ActiveUpdate)
        {
            rightMaxDegress = 180f - maxDegrees;
            rightMinDegress = -180f - minDegrees;
        }

        if (Math.Sign(targetObj.transform.localScale.x) == 1)
        {
            if (deg > maxDegrees || deg < minDegrees)
            {
                playerIndeg = false;
                if (Math.Sign(deg) == 1 || Math.Sign(deg) == 0)
                {
                    deg = maxDegrees;
                }
                else
                {
                    deg = minDegrees;
                }
            }
            else
            {
                playerIndeg = true;
            }
        }
        else
        {
            if (deg < rightMaxDegress && deg > rightMinDegress)
            {
                playerIndeg = false;
                if (Math.Sign(deg) == 1 || Math.Sign(deg) == 0)
                {
                    deg = rightMaxDegress;
                }
                else
                {
                    deg = rightMinDegress;
                }
            }
            else
            {
                playerIndeg = true;
            }
        }

        return DegToVector(deg);
    }

    IEnumerator RayUpdate()
    {
        while (true)
        {
            //FixedUpdate後でないとバグるので終わるまで待ってから処理をする
            yield return new WaitForFixedUpdate();

            //対象オブジェクトの向きに合わせてレイのポジションを変える
            if (Math.Sign(targetObj.transform.localScale.x) == 1)
            {
                position = targetObj.transform.position + rightPositionDifference;
            }
            else
            {
                position = targetObj.transform.position + leftPositionDifference;
            }

            ray = new Ray(position, PlayerController.playerPosition - position);

            //角度を指定された範囲に修正
            if (setRayScopeArea)
            {
                if (rayFrontOnly)
                {
                    ray.direction = EditingVector2FrontOnly(ray.direction);
                }
                else
                {
                    ray.direction = EditingVector2(ray.direction);
                }
            }

            if (playerIndeg)
            {
                hit = Physics2D.Raycast(ray.origin, ray.direction, rayDistance, layerMask);
            }


            //対象のタグと接触したか調べる
            #region
            if (playerIndeg && hit == true)
            {
                foreach (var s in targetTags)
                {
                    if (hit.collider.tag == s)
                    {
                        inTarget = true;
                        break;
                    }
                    else
                    {
                        inTarget = false;
                    }
                }
            }
            else
            {
                inTarget = false;
            }
            #endregion

            if (playerIndeg)
            {
                Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
            }

            //角度範囲のレイ表示
            #region
            if (showRayScope && setRayScopeArea)
            {

                if (rayFrontOnly)
                {
                    if (Math.Sign(targetObj.transform.localScale.x) == 1)
                    {
                        Debug.DrawRay(ray.origin, DegToVector(maxDegrees), Color.green);
                        Debug.DrawRay(ray.origin, DegToVector(minDegrees), Color.green);
                    }
                    else
                    {
                        Debug.DrawRay(ray.origin, DegToVector(rightMaxDegress), Color.green);
                        Debug.DrawRay(ray.origin, DegToVector(rightMinDegress), Color.green);
                    }
                }
                else
                {
                    Debug.DrawRay(ray.origin, DegToVector(maxDegrees), Color.green);
                    Debug.DrawRay(ray.origin, DegToVector(minDegrees), Color.green);
                    Debug.DrawRay(ray.origin, DegToVector(rightMaxDegress), Color.green);
                    Debug.DrawRay(ray.origin, DegToVector(rightMinDegress), Color.green);
                }
            }
            #endregion
        }
    }

    private float VectorToRad(Vector2 thisVec)
    {
        return Mathf.Atan2(thisVec.y, thisVec.x);
    }
    private Vector2 RadToVector(float thisFloat)
    {
        return new Vector2(Mathf.Cos(thisFloat), Mathf.Sin(thisFloat));
    }
    private float VectorToDeg(Vector2 thisVec)
    {
        return Mathf.Atan2(thisVec.y, thisVec.x) * Mathf.Rad2Deg;
    }
    private Vector2 DegToVector(float thisFloat)
    {
        return new Vector2(Mathf.Cos(thisFloat * Mathf.Deg2Rad), Mathf.Sin(thisFloat * Mathf.Deg2Rad));
    }
}
