using System;

namespace Firebend.AutoCrud.Core.Attributes;

/// <summary>
/// Use this attribute to annotate an object's property.
/// When annotated it will ignore updates when sent to the Firebend Auto Crud controllers.
/// </summary>
public class AutoCrudIgnoreUpdate : Attribute;
