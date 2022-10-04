# Entity-Collision-Component-For-Unity
C# class for use in Unity2D made to handle collision detection for a character controller.


> Compatible with CapsuleCollider2D, BoxCollider2D, CircleCollider2D.

> Collision Component checks take into account the size, offset, and rotation of collider when running checks.

> Uses Physics2D.OverlapBoxAll or Physics2D.OverlapCircleAll to check around the collider of the object. 

> Uses Physics2D.Raycast to cast 3 rays along the bottom of the collider to get detailed info about the collider the object is on. The RaycastHit2D can be retrieved from each of the three rays as well as the slope from a specific ray or the average slope of all the rays that have hit something. 

> Contains functionality to ignore all colliders on a layer within a range of object. This will allow the object to pass through specific objects. Will also re-enable collision with those objects once out of range.


Forward ray will be the ray in front of the collider relative to rigidbody.transform.localscale.x. If localscale.x == -1 then forward facing ray will be on the left side. This makes it easier to get the ray in front of the collider.

Check setting will fall through to the next lower setting if the specified setting fails. This is for cases when the collider is on the edge of the ground. If setting is 3 (3 rays needed to be on slope) but the collider is hanging off the edge of a slope with no ground below, then setting will automatically fall to setting 2 then 1 if 2 fails.


UpdateSlopeCheck - Use this to update the 3 raycasts used to get ground info. This function will update the ray positions, rotation, and cast them
>check dist arg is the length of the rays
>slope threshold is the minimum angle that is considered a slope. This will be used to determine object is on a slope or not. (setting to 5 will mean that object is technically not on a slope if ground angle is == 3)
>check setting determines the way is on slope is set and ranges from [1-3]. Each setting sets how many rays need to be on a slope for object to be on a slope (setting to 2 will require the center ray and 1 other to be hitting a slope higher than slopeThreshold to set isOnSlope to true)
>useRotation determines if rotation of collider should be used for casting rays. Leave false for rays to always cast down)

GetGroundData - returns the RaycastHit2D of specified ray position. Allowing fallthrough will make the function return the next available hit if specified ray hit nothing. Fallthrough order is forward, center, back.

GetGroundNormal - returns the normal from RaycastHit2D of the specified ray. If Average is passed as an argument then the average normal of all rays that hit something will be returned. Uses GetGroundData to get the normal.

GetGroundAngle - uses GetGroundNormal then calculates angle using Vector2.Angle or Vector2.SignedAngle depending on signed argument. 

GetGroundPerpendicular - returns Vector2.Perpendicular of GetGroundNormal

IsRaycastValid - returns if specified ray hit anything. No fallthrough.

IsAngleAboveThreshold - function used to check if angle of the ground is larger than passed in ‘threshold’ argument. Compares the angle using GetGroundAngle, unsigned, allowing fallthrough. CheckSetting argument acts the same as in UpdateSlopeCheck and determines how GetGroundAngle is called. 3 - all rays hit ground, gets average angle. 2 - center ray and one other hit ground, uses center ray with fallthrough. 1 - one ray hit ground, uses forward ray with fallthrough.

Get___CheckPosition functions - these return the position of the center, upper, or lower points of the collider taking into the account of the check size making it easy to find where the check should be without being too far out of the collider.

IgnoreCollidersInRange - adds colliders in Physics2D.OverlapCircleAll to an ignore list and sets collider to ignore colliders added to the list.

ReenableIgnoredColliders - sets collider to no longer ignore collisions with colliders in ignoreList. Will not re-enable collision if other collider is in collider. (use with IgnoreCollidersInRange to allow collider to pass through objects temporarily or permanently)
