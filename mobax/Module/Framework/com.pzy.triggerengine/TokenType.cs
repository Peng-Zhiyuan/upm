
public enum TokenType
{
    Variable,

    /// <summary>
    /// 双目运算符
    /// </summary>
    BinocularOperatorLv3,
    BinocularOperatorLv2,
    BinocularOperatorLv1,

    IntConst,
    StringConst,
    BoolConst,
    StartQout,
    EndQout,

    ResolveResult,

    Word,
    Comments,
    Unknown,
}