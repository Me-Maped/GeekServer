﻿using MessagePack;

namespace Geek.Server.Proto
{

    [MessagePackObject(true)]
    [Serialize(111111)]
    public class A
    {
        public int Age { get; set; }
    }

    [MessagePackObject(true)]
    [Serialize(111112)]
    public class B : A
    {
        public string Name { get; set; }
        [IgnoreMember]
        public string Test { get; set; }
    }


    /// <summary>
    /// 玩家基础信息
    /// </summary>
    [MessagePackObject(true)]
    [Serialize(111000)]
    public class UserInfo
    {
        /// <summary>
        /// 角色名
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 角色等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }
        /// <summary>
        /// vip等级
        /// </summary>
        public int VipLevel { get; set; }
    }

    /// <summary>
    /// 请求登录
    /// </summary>
    [MessagePackObject(true)]
    [Serialize(111001)]
    public class ReqLogin : Message
    {
        public string UserName { get; set; }
        public string Platform { get; set; }
        public int SdkType { get; set; }
        public string SdkToken { get; set; }
        public string Device { get; set; }
    }


    /// <summary>
    /// 请求登录
    /// </summary>
    [MessagePackObject(true)]
    [Serialize(111002)]
    public class ResLogin : Message
    {
        /// <summary>
        /// 登陆结果，0成功，其他时候为错误码
        /// </summary>
        public int Code { get; set; }
        public UserInfo UserInfo { get; set; }
    }


    /// <summary>
    /// 等级变化
    /// </summary>
    [MessagePackObject(true)]
    [Serialize(111003)]
    public class ResLevelUp : Message
    {
        /// <summary>
        /// 玩家等级
        /// </summary>
        public int Level { get; set; }
    }

    /// <summary>
    /// 双向心跳/收到恢复同样的消息
    /// </summary>
    [MessagePackObject(true)]
    [Serialize(111004)]
    public class HearBeat : Message
    {
        /// <summary>
        /// 当前时间
        /// </summary>
        public long TimeTick { get; set; }
    }

    /// <summary>
    /// 客户端每次请求都会回复错误码
    /// </summary>
    [MessagePackObject(true)]
    [Serialize(111005)]
    public class ResErrorCode : Message
    {
        /// <summary>
        /// 0:表示无错误
        /// </summary>
        public long ErrCode { get; set; }
        /// <summary>
        /// 错误描述（不为0时有效）
        /// </summary>
        public string Desc { get; set; }
    }

}
