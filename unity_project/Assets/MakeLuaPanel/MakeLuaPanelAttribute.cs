using System;
using UnityEngine;

/// <summary>
/// 标记字段不要在lua文件中生成对应的内容
/// Mark the field that will not be converted in the generated content
/// </summary>
public class MLPException : Attribute
{
    public MLPException()
    {
    }

}

/// <summary>
/// 为字段标记注释，这个注释文本将会在生成lua文件时，在此字段所指向的物体在lua里对应的赋值代码附近生成对应的lua注释
/// Mark the field that some annotation will be generated when converts
/// </summary>
public class MLPAnnotation : Attribute
{
    /// <summary>
    /// 注释如何添加
    /// </summary>
    public enum AddTypeEnum
    {
        /// <summary>
        /// 加在相关代码上面
        /// add annotation above the code
        /// </summary>
        Above = 0,
        /// <summary>
        /// 加在相关代码同行之后，但是当注释文本为多行的情况下，将自动切换到Above模式
        /// add annotation at the end of the code, notice that if the annotation is multiline, AddTypeEnum.Append will not be appled but AddTypeEnum.Above will
        /// </summary>
        Append = 1,
    }
    private string mAnnotation;
    private AddTypeEnum mAddType = AddTypeEnum.Above;
    public MLPAnnotation(string annotation, AddTypeEnum addType = AddTypeEnum.Above)
    {
        mAnnotation = annotation;
        mAddType = addType;
    }

    public string Annotion
    {
        get
        {
            return mAnnotation;
        }
    }
    public AddTypeEnum AnnotationAddType
    {
        get
        {
            return mAddType;
        }
    }
}

public class MLPDivisionBelow:PropertyAttribute
{

}