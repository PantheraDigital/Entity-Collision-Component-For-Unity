using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace EntityController2D
{

    public enum RaycastData
    {
        Forward,
        Center,
        Back,
        Average
    }

    [System.Serializable]
    public class CollisionCheckComponent
    {
        //list of compatable collider types for this class
        enum ColliderTypes
        {
            Collider2D,
            CapsuleCollider2D,
            BoxCollider2D,
            CircleCollider2D
        }


        ColliderTypes m_colliderType = ColliderTypes.Collider2D;

        List<Collider2D> m_collisionIgnoredEntities = new List<Collider2D>();

        Rigidbody2D m_rigidBody;
        Collider2D m_collider;

        LayerMask m_layers;
        string m_ignoreTags = "";

        RaycastHit2D m_centerHit;
        RaycastHit2D m_frontHit;
        RaycastHit2D m_backHit;

        Vector2 m_centerSlopeCheck;
        Vector2 m_leftSlopeCheck;
        Vector2 m_rightSlopeCheck;

        bool m_isSlopePositive;
        bool m_isOnSlope;

        public bool m_drawDebug;



        public CollisionCheckComponent(Rigidbody2D rigidBody, Collider2D collider, LayerMask layers, string tags)
        {
            m_rigidBody = rigidBody;
            m_collider = collider;
            m_layers = layers;
            m_ignoreTags = tags;

            if (collider.GetType() == typeof(CapsuleCollider2D))
            {
                //Debug.Log("Capsule");
                m_colliderType = ColliderTypes.CapsuleCollider2D;
            }
            else if (collider.GetType() == typeof(BoxCollider2D))
            {
                //Debug.Log("Box");
                m_colliderType = ColliderTypes.BoxCollider2D;
            }
            else if (collider.GetType() == typeof(CircleCollider2D))
            {
                //Debug.Log("Circle");
                m_colliderType = ColliderTypes.CircleCollider2D;
            }
            else
                Debug.Log("Unsupported collder type passed to " + this.ToString());
        }

        public void UpdateSlopeCheck(float checkDist, float slopeThreshold, int checkSetting, bool useRotation = false)
        {
            Vector2 direction = Vector3.down;
            Quaternion rotation = Quaternion.Euler(0f, 0f, m_rigidBody.rotation);

            //update ray positions
            Vector3 rayOffset = GetCollider2DOffset();
            float trueRightOffset = (GetCollider2DSize().x / 2f) - 0.01f;//0.01f is skin depth. change to move rays inwards more
            float trueLeftOffset = -(GetCollider2DSize().x / 2f) + 0.01f;

            if (m_isOnSlope && useRotation)
            {
                direction = rotation * Vector3.down;

                m_centerSlopeCheck = (Vector3)m_rigidBody.position + (rotation * rayOffset);

                if (m_rigidBody.transform.localScale.x >= 0)
                {
                    m_leftSlopeCheck = (Vector3)m_rigidBody.position + (rotation * new Vector3(rayOffset.x + trueLeftOffset, rayOffset.y, rayOffset.z));
                    m_rightSlopeCheck = (Vector3)m_rigidBody.position + (rotation * new Vector3(rayOffset.x + trueRightOffset, rayOffset.y, rayOffset.z));
                }
                else
                {
                    m_leftSlopeCheck = (Vector3)m_rigidBody.position + (rotation * new Vector3(rayOffset.x + trueRightOffset, rayOffset.y, rayOffset.z));
                    m_rightSlopeCheck = (Vector3)m_rigidBody.position + (rotation * new Vector3(rayOffset.x + trueLeftOffset, rayOffset.y, rayOffset.z));
                }
            }
            else
            {
                m_centerSlopeCheck = (Vector3)m_rigidBody.position + rayOffset;

                if (m_rigidBody.transform.localScale.x >= 0)
                {
                    m_leftSlopeCheck = (Vector3)m_rigidBody.position + new Vector3(rayOffset.x + trueLeftOffset, rayOffset.y, rayOffset.z);
                    m_rightSlopeCheck = (Vector3)m_rigidBody.position + new Vector3(rayOffset.x + trueRightOffset, rayOffset.y, rayOffset.z);
                }
                else
                {
                    m_leftSlopeCheck = (Vector3)m_rigidBody.position + new Vector3(rayOffset.x + trueRightOffset, rayOffset.y, rayOffset.z);
                    m_rightSlopeCheck = (Vector3)m_rigidBody.position + new Vector3(rayOffset.x + trueLeftOffset, rayOffset.y, rayOffset.z);
                }

            }

            //cast rays
            RaycastHit2D centerHit = Physics2D.Raycast(m_centerSlopeCheck, direction, checkDist, m_layers);
            RaycastHit2D leftHit = Physics2D.Raycast(m_leftSlopeCheck, direction, checkDist, m_layers);
            RaycastHit2D rightHit = Physics2D.Raycast(m_rightSlopeCheck, direction, checkDist, m_layers);
            m_centerHit = centerHit;
            m_frontHit = rightHit;
            m_backHit = leftHit;


            SetSlopeInfo(slopeThreshold, checkSetting);

            if (useRotation && !m_centerHit)
            {
                UpdateSlopeCheck(checkDist, slopeThreshold, checkSetting, false);
            }

            if (m_drawDebug)
            {
                //Draw raycasts
                Debug.DrawRay(m_leftSlopeCheck, direction * checkDist, Color.red);
                Debug.DrawRay(m_centerSlopeCheck, direction * checkDist, Color.magenta);
                Debug.DrawRay(m_rightSlopeCheck, direction * checkDist, Color.blue);

                //Draw ground angles
                //Debug.DrawRay(m_hit.point, m_slopeNormPerp, Color.red);
                //Debug.DrawRay(m_hit.point, m_hit.normal, Color.green);
                //Debug.DrawRay(m_leftSlopeCheck, Vector2.up, Color.red); 
                //Debug.Log("Norm: " + GetAverageNormal() + " SlopeDownAngle: " + GetGroundAngle() + " IsOnSlope: " + IsOnSLope());

                Debug.Log(GetGroundAngle(RaycastData.Back) + "  " + GetGroundAngle(RaycastData.Center) + "  " + GetGroundAngle(RaycastData.Forward));
            }
        }

        #region GettersAndSetters
        public void SetRigidBody(Rigidbody2D rigidBody)
        {
            m_rigidBody = rigidBody;
        }
        public Rigidbody2D GetRigidbody2D()
        {
            return m_rigidBody;
        }

        public void SetCollider2D(Collider2D collider)
        {
            m_collider = collider;
        }
        public Collider2D GetCollider2D()
        {
            return m_collider;
        }

        public Vector2 GetCollider2DSize()
        {
            if (m_colliderType == ColliderTypes.CapsuleCollider2D)
            {
                return ((CapsuleCollider2D)m_collider).size;
            }
            else if (m_colliderType == ColliderTypes.BoxCollider2D)
            {
                return ((BoxCollider2D)m_collider).size;
            }
            else if (m_colliderType == ColliderTypes.CircleCollider2D)
            {
                float size = ((CircleCollider2D)m_collider).radius;
                return new Vector2(size, size);
            }

            return Vector2.zero;
        }
        public void SetCollider2DSize(Vector2 size)
        {
            if (m_colliderType == ColliderTypes.CapsuleCollider2D)
            {
                ((CapsuleCollider2D)m_collider).size = size;
            }
            else if (m_colliderType == ColliderTypes.BoxCollider2D)
            {
                ((BoxCollider2D)m_collider).size = size;
            }
            else if (m_colliderType == ColliderTypes.CircleCollider2D)
            {
                ((CircleCollider2D)m_collider).radius = size.x;
            }
        }

        public Vector2 GetCollider2DOffset()
        {
            if (m_colliderType == ColliderTypes.CapsuleCollider2D)
            {
                return ((CapsuleCollider2D)m_collider).offset;
            }
            else if (m_colliderType == ColliderTypes.BoxCollider2D)
            {
                return ((BoxCollider2D)m_collider).offset;
            }
            else if (m_colliderType == ColliderTypes.CircleCollider2D)
            {
                return ((CircleCollider2D)m_collider).offset;
            }

            return Vector2.zero;
        }
        public void SetCollider2DOffset(Vector2 offset)
        {
            if (m_colliderType == ColliderTypes.CapsuleCollider2D)
            {
                ((CapsuleCollider2D)m_collider).offset = offset;
            }
            else if (m_colliderType == ColliderTypes.BoxCollider2D)
            {
                ((BoxCollider2D)m_collider).offset = offset;
            }
            else if (m_colliderType == ColliderTypes.CircleCollider2D)
            {
                ((CircleCollider2D)m_collider).offset = offset;
            }
        }

        public void SetCollisionLayers(LayerMask layers)
        {
            m_layers = layers;
        }
        public void SetIgnoreTags(string tags)
        {
            m_ignoreTags = tags;
        }

        public Vector2 GetRaycastPos()
        {
            return m_centerSlopeCheck;
        }

        public bool IsSlopePositive()
        {
            return m_isSlopePositive;
        }

        public bool IsOnSLope()
        {
            return m_isOnSlope;
        }

        public RaycastHit2D GetGroundData(RaycastData rayType = RaycastData.Center, bool allowFallThrough = false)
        {
            switch (rayType)
            {
                case RaycastData.Forward:
                    if (allowFallThrough)
                    {
                        if (m_frontHit)
                            return m_frontHit;
                        else
                            goto case RaycastData.Center;
                    }
                    else
                        return m_frontHit;

                case RaycastData.Center:
                    if (allowFallThrough)
                    {
                        if (m_centerHit)
                            return m_centerHit;
                        else
                            goto case RaycastData.Back;
                    }
                    else
                        return m_centerHit;

                case RaycastData.Back:
                    if (allowFallThrough)
                    {
                        if (m_backHit)
                            return m_backHit;
                        else
                            return new RaycastHit2D();
                    }
                    else
                        return m_backHit;

                default:
                    return m_centerHit;
            }
        }

        public Vector2 GetGroundNormal(RaycastData rayType = RaycastData.Average, bool allowFallThrough = false)
        {
            if (rayType == RaycastData.Forward || rayType == RaycastData.Center || rayType == RaycastData.Back)
                return GetGroundData(rayType, allowFallThrough).normal;
            else if (rayType == RaycastData.Average)
                return GetAverageNormal();
            else
                return Vector2.zero;
        }

        public float GetGroundAngle(RaycastData rayType = RaycastData.Average, bool signed = false, bool allowFallThrough = false)
        {
            Vector2 norm = GetGroundNormal(rayType, allowFallThrough);
            if (signed)
                return Vector2.SignedAngle(norm, Vector2.up);
            else
                return Vector2.Angle(norm, Vector2.up);
        }

        //allows fall through
        public Vector2 GetGroundPerpendicular(RaycastData rayType = RaycastData.Average)
        {
            return Vector2.Perpendicular(GetGroundNormal(rayType, true));
        }

        public bool IsRaycastValid(RaycastData rayType)
        {
            switch (rayType)
            {
                case RaycastData.Forward:
                    if (m_frontHit)
                        return true;
                    else
                        return false;

                case RaycastData.Center:
                    if (m_centerHit)
                        return true;
                    else
                        return false;

                case RaycastData.Back:
                    if (m_backHit)
                        return true;
                    else
                        return false;

                case RaycastData.Average:
                    if (m_backHit || m_centerHit || m_frontHit)
                        return true;
                    else
                        return false;

                default:
                    return false;
            }
        }

        public bool IsAngleAboveThreshold(float threshold, int checkSetting)
        {
            switch (checkSetting)
            {
                case 3:
                    if (m_centerHit && m_backHit && m_frontHit)
                        return GetGroundAngle(RaycastData.Average, false, true) > threshold;
                    else
                        goto case 2;

                case 2:
                    if ((m_centerHit && m_backHit) || (m_centerHit && m_frontHit))
                        return GetGroundAngle(RaycastData.Center, false, true) > threshold;
                    else
                        goto case 1;

                case 1:
                    if (m_centerHit || m_backHit || m_frontHit)
                        return GetGroundAngle(RaycastData.Forward, false, true) > threshold;
                    else
                        return false;

                default:
                    goto case 2;
            }
        }

        public Vector2 GetLowerCheckPosition(float checkRadius, float checkOffset, Vector2 colliderSize, Vector2 colliderOffset)
        {
            Vector2 offset = new Vector2(colliderOffset.x, colliderOffset.y - (colliderSize.y * 0.5f) + (checkRadius * checkOffset));
            if (m_isOnSlope)
                offset = Quaternion.Euler(0f, 0f, m_rigidBody.rotation) * offset;

            return m_rigidBody.position + offset;
        }
        public Vector2 GetLowerCheckPosition(float checkRadius, float checkOffset)
        {
            return GetLowerCheckPosition(checkRadius, checkOffset, GetCollider2DSize(), GetCollider2DOffset());
        }
        public Vector2 GetUpperCheckPosition(float checkRadius, float checkOffset, Vector2 colliderSize, Vector2 colliderOffset)
        {
            Vector2 offset = new Vector2(colliderOffset.x, colliderOffset.y + (colliderSize.y * 0.5f) - (checkRadius * checkOffset));
            if (m_isOnSlope)
                offset = Quaternion.Euler(0f, 0f, m_rigidBody.rotation) * offset;

            return m_rigidBody.position + offset;
        }
        public Vector2 GetUpperCheckPosition(float checkRadius, float checkOffset)
        {
            return GetUpperCheckPosition(checkRadius, checkOffset, GetCollider2DSize(), GetCollider2DOffset());
        }
        public Vector2 GetMiddleCheckPosition(Vector2 colliderOffset)
        {
            return new Vector2(m_rigidBody.position.x, m_rigidBody.position.y + colliderOffset.y);
        }
        public Vector2 GetMiddleCheckPosition()
        {
            return GetMiddleCheckPosition(GetCollider2DOffset());
        }

        Vector2 GetAverageNormal()
        {
            int div = 0;
            Vector2 avg = Vector2.zero;

            if (m_backHit)
            {
                div += 1;
                avg += m_backHit.normal;
            }

            if (m_frontHit)
            {
                div += 1;
                avg += m_frontHit.normal;
            }

            if (m_centerHit)
            {
                div += 1;
                avg += m_centerHit.normal;
            }

            if (div > 1)
                avg /= div;

            return avg;
        }
        #endregion

        #region Checks
        public bool CheckLower(float checkSize, float checkOffset, bool useSquare = false)
        {
            return Check(GetLowerCheckPosition(checkSize, checkOffset), checkSize, useSquare);
        }
        public bool CheckLower(float checkSize, float checkOffset, Vector2 colliderSize, Vector2 colliderOffset, bool useSquare = false)
        {
            return Check(GetLowerCheckPosition(checkSize, checkOffset, colliderSize, colliderOffset), checkSize, useSquare);
        }

        public bool CheckUpper(float checkSize, float checkOffset, bool useSquare = false)
        {
            return Check(GetUpperCheckPosition(checkSize, checkOffset), checkSize, useSquare);
        }
        public bool CheckUpper(float checkSize, float checkOffset, Vector2 colliderSize, Vector2 colliderOffset, bool useSquare = false)
        {
            return Check(GetUpperCheckPosition(checkSize, checkOffset, colliderSize, colliderOffset), checkSize, useSquare);
        }

        public bool CheckMiddle(float checkSize, bool useSquare = false)
        {
            return Check(GetMiddleCheckPosition(), checkSize, useSquare);
        }
        public bool CheckMiddle(float checkSize, Vector2 colliderOffset, bool useSquare = false)
        {
            return Check(GetMiddleCheckPosition(colliderOffset), checkSize, useSquare);
        }

        bool Check(Vector2 position, float checkSize, bool useSquare)
        {
            return OverlapCheck(position, checkSize, m_layers, useSquare);
        }

        public Collider2D OverlapCheck(Vector2 position, float checkSize, LayerMask layers, bool useSquare)
        {
            Collider2D[] results;

            if (useSquare)
            {
                results = Physics2D.OverlapBoxAll(position, new Vector2(checkSize, checkSize), 0f, layers);
            }
            else
            {
                results = Physics2D.OverlapCircleAll(position, checkSize, layers);
            }

            if (results != null && results.Length > 0)
            {
                foreach (Collider2D collider in results)
                {
                    if (collider != null && collider != m_collider && !m_ignoreTags.Contains(collider.tag))
                    {
                        return collider;
                    }
                }
            }

            return null;
        }
        #endregion

        void SetSlopeInfo(float slopeThreshold, int checkSetting)
        {
            //check for slope
            if (m_centerHit || m_backHit || m_frontHit)
            {
                switch (checkSetting)
                {
                    case 3:
                        {
                            if (m_centerHit && m_backHit && m_frontHit)
                            {
                                float leftAngle = GetGroundAngle(RaycastData.Back);
                                float middleAngle = GetGroundAngle(RaycastData.Center);
                                float rightAngle = GetGroundAngle(RaycastData.Forward);

                                if (leftAngle >= slopeThreshold && middleAngle >= slopeThreshold && rightAngle >= slopeThreshold)
                                {
                                    m_isOnSlope = true;
                                }
                                else
                                {
                                    m_isOnSlope = false;
                                }

                                break;
                            }
                            else
                                goto case 2;
                        }
                    case 2:
                        {
                            if ((m_centerHit && m_backHit) || (m_centerHit && m_frontHit))
                            {
                                if (GetGroundAngle(RaycastData.Center) >= slopeThreshold)
                                {
                                    if ((m_backHit && GetGroundAngle(RaycastData.Back) >= slopeThreshold) || (m_frontHit && GetGroundAngle(RaycastData.Forward) >= slopeThreshold))
                                    {
                                        m_isOnSlope = true;
                                    }
                                    else
                                        m_isOnSlope = false;
                                }
                                else
                                    m_isOnSlope = false;

                                break;
                            }
                            else
                                goto case 1;
                        }
                    case 1:
                        {
                            if (m_backHit && GetGroundAngle(RaycastData.Back) >= slopeThreshold)
                            {
                                m_isOnSlope = true;
                            }
                            else if (m_centerHit && GetGroundAngle(RaycastData.Center) >= slopeThreshold)
                            {
                                m_isOnSlope = true;
                            }
                            else if (m_frontHit && GetGroundAngle(RaycastData.Forward) >= slopeThreshold)
                            {
                                m_isOnSlope = true;
                            }
                            else
                                m_isOnSlope = false;

                            break;
                        }
                    default:
                        {
                            Debug.LogWarning(m_rigidBody.gameObject.name + " slope check setting out of range " + checkSetting);
                            goto case 2;
                        }
                }

                if (GetGroundAngle(RaycastData.Average, true) >= 0f)
                    m_isSlopePositive = false;
                else
                    m_isSlopePositive = true;

            }
            else
            {
                m_isOnSlope = false;
                m_isSlopePositive = false;
            }

        }

        public void IgnoreCollidersInRange(GameObject self, LayerMask layerToCheck, float range, string tags = "")
        {
            //if a self is in range of colliders in layerToCheck disable collision with them and add to m_collisionIgnoredEntities
            Collider2D[] colliders = Physics2D.OverlapCircleAll(self.transform.position, range, layerToCheck);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != self)
                {
                    if (tags == "")
                    {
                        Physics2D.IgnoreCollision(colliders[i], self.GetComponent<Collider2D>(), true);
                        m_collisionIgnoredEntities.Add(colliders[i]);
                    }
                    else if (tags.Contains(colliders[i].gameObject.tag))
                    {
                        Physics2D.IgnoreCollision(colliders[i], self.GetComponent<Collider2D>(), true);
                        m_collisionIgnoredEntities.Add(colliders[i]);
                    }
                }
            }

        }

        public void ReenableIgnoredColliders(GameObject self, LayerMask layerToCheck)
        {
            if (m_collisionIgnoredEntities.Count != 0)
            {
                //get collisions within collider
                Collider2D[] colliders = Physics2D.OverlapBoxAll(new Vector2(self.transform.position.x + GetCollider2DOffset().x, self.transform.position.y + GetCollider2DOffset().y), GetCollider2DSize(), layerToCheck);

                //loop through ignore list
                for (int i = 0; i < m_collisionIgnoredEntities.Count; i++)
                {
                    //if objects in ignore list are not in collider then restor collision
                    if (!colliders.Contains(m_collisionIgnoredEntities[i]))
                    {
                        Physics2D.IgnoreCollision(m_collisionIgnoredEntities[i], self.GetComponent<Collider2D>(), false);
                        m_collisionIgnoredEntities.Remove(m_collisionIgnoredEntities[i]);
                    }
                }
            }
        }
    }

}