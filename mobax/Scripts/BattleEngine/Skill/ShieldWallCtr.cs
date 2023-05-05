namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class ShieldWallCtr
    {
        private static List<ShieldWallEntity> shieldWalls = new List<ShieldWallEntity>();

        public static void CreatShieldWall(CreatShieldWallData taskData, CombatActorEntity owner)
        {
            var shieldWall = Entity.CreateWithParent<ShieldWallEntity>(MasterEntity.Instance, "");
            if (taskData.isAttachLookAt)
            {
                Vector3 pos = Quaternion.Euler(owner.GetEulerAngles()) * taskData.offset + owner.GetPosition();
                Vector3 rot = Quaternion.Euler(taskData.angleOffset).eulerAngles;
                shieldWall.Born(pos, rot);
            }
            else
            {
                Vector3 pos = taskData.offset + owner.GetPosition();
                Vector3 rot = Quaternion.Euler(taskData.angleOffset).eulerAngles;
                shieldWall.Born(pos, rot);
            }
            shieldWall.Init(taskData, owner);
            shieldWall.AddComponent<UpdateComponent>();
            shieldWalls.Add(shieldWall);
        }

        public static bool BulletCheckTrigger(BulletEntity bullet)
        {
            if (bullet == null)
            {
                return true;
            }
            for (int i = 0; i < shieldWalls.Count; i++)
            {
                if (shieldWalls[i] == null) //|| shieldWalls[i].lTransform == null
                {
                    shieldWalls.RemoveAt(i);
                    i--;
                    continue;
                }
                if (shieldWalls[i].times <= 0)
                {
                    Entity.Destroy(shieldWalls[i]);
                    shieldWalls.RemoveAt(i);
                    i--;
                    continue;
                }
                if (shieldWalls[i].Owner != null
                    && shieldWalls[i].Owner.isAtker == bullet.Owner.isAtker)
                {
                    continue;
                }
                if (shieldWalls[i].wallTYpe == SHIELD_WALL_TYPE.Cube)
                {
                    Vector3 bulletPos = bullet.GetPosition();
                    float maxH = Mathf.Max(bulletPos.y + bullet.bulletHeight, shieldWalls[i].GetPosition().y + shieldWalls[i].size.y / 2);
                    float minH = Mathf.Min(bulletPos.y - bullet.bulletHeight, shieldWalls[i].GetPosition().y - shieldWalls[i].size.y / 2);
                    if (maxH - minH < shieldWalls[i].size.y + bullet.bulletHeight * 2)
                    {
                        Vector3 unitPos = shieldWalls[i].World2Local(bulletPos);
                        Vector2 circleCenter = new Vector2(unitPos.x, unitPos.z);
                        Vector2 rtCenter = new Vector2(shieldWalls[i].GetPosition().x, shieldWalls[i].GetPosition().z + shieldWalls[i].size.z / 2);
                        Vector2 atkDir = new Vector2(Mathf.Abs(circleCenter.x - rtCenter.x), Mathf.Abs(circleCenter.y - rtCenter.y));
                        Vector2 rectDia = new Vector2(shieldWalls[i].size.x / 2, shieldWalls[i].size.z / 2);
                        Vector2 v = atkDir - rectDia;
                        v = new Vector2(Mathf.Max(v.x, 0), Mathf.Max(v.y, 0));
                        if (v.magnitude < bullet.bulletHeight)
                        {
                            shieldWalls[i].times -= 1;
                            shieldWalls[i].CreatHitEffect(bullet.GetPosition() + bullet.GetForward() * bullet.bulletHeight);
                            return true;
                        }
                    }
                }
                else if (shieldWalls[i].wallTYpe == SHIELD_WALL_TYPE.Cylinder)
                {
                    float dis = MathHelper.DistanceVec3(bullet.GetPosition(), shieldWalls[i].GetPosition());
                    if (dis < shieldWalls[i].radius + bullet.bulletHeight)
                    {
                        shieldWalls[i].times -= 1;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}