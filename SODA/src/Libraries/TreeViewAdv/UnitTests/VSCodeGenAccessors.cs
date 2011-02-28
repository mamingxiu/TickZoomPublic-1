// ------------------------------------------------------------------------------
//<autogenerated>
//        This code was generated by Microsoft Visual Studio Team System 2005.
//
//        Changes to this file may cause incorrect behavior and will be lost if
//        the code is regenerated.
//</autogenerated>
//------------------------------------------------------------------------------
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aga.Controls.UnitTests
{
[System.Diagnostics.DebuggerStepThrough()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TestTools.UnitTestGeneration", "1.0.0.0")]
internal class BaseAccessor {
    
    protected Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject m_privateObject;
    
    protected BaseAccessor(object target, Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType type) {
        m_privateObject = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(target, type);
    }
    
    protected BaseAccessor(Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType type) : 
            this(null, type) {
    }
    
    internal virtual object Target {
        get {
            return m_privateObject.Target;
        }
    }
    
    public override string ToString() {
        return this.Target.ToString();
    }
    
    public override bool Equals(object obj) {
        if (typeof(BaseAccessor).IsInstanceOfType(obj)) {
            obj = ((BaseAccessor)(obj)).Target;
        }
        return this.Target.Equals(obj);
    }
    
    public override int GetHashCode() {
        return this.Target.GetHashCode();
    }
}


[System.Diagnostics.DebuggerStepThrough()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TestTools.UnitTestGeneration", "1.0.0.0")]
internal class Aga_Controls_Tree_TreeNodeAdvAccessor : BaseAccessor {
    
    protected static Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType m_privateType = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType(typeof(global::Aga.Controls.Tree.TreeNodeAdv));
    
    internal Aga_Controls_Tree_TreeNodeAdvAccessor(global::Aga.Controls.Tree.TreeNodeAdv target) : 
            base(target, m_privateType) {
    }
    
    internal global::Aga.Controls.Tree.TreeViewAdv _tree {
        get {
            global::Aga.Controls.Tree.TreeViewAdv ret = ((global::Aga.Controls.Tree.TreeViewAdv)(m_privateObject.GetField("_tree")));
            return ret;
        }
        set {
            m_privateObject.SetField("_tree", value);
        }
    }
    
    internal global::Aga.Controls.Tree.TreeViewAdv Tree {
        get {
            global::Aga.Controls.Tree.TreeViewAdv ret = ((global::Aga.Controls.Tree.TreeViewAdv)(m_privateObject.GetProperty("Tree")));
            return ret;
        }
    }
    
    internal int _row {
        get {
            int ret = ((int)(m_privateObject.GetField("_row")));
            return ret;
        }
        set {
            m_privateObject.SetField("_row", value);
        }
    }
    
    internal int Row {
        get {
            int ret = ((int)(m_privateObject.GetProperty("Row")));
            return ret;
        }
        set {
            m_privateObject.SetProperty("Row", value);
        }
    }
    
    internal int _index {
        get {
            int ret = ((int)(m_privateObject.GetField("_index")));
            return ret;
        }
        set {
            m_privateObject.SetField("_index", value);
        }
    }
    
    internal bool _isSelected {
        get {
            bool ret = ((bool)(m_privateObject.GetField("_isSelected")));
            return ret;
        }
        set {
            m_privateObject.SetField("_isSelected", value);
        }
    }
    
    internal bool IsVisible {
        get {
            bool ret = ((bool)(m_privateObject.GetProperty("IsVisible")));
            return ret;
        }
    }
    
    internal bool _isLeaf {
        get {
            bool ret = ((bool)(m_privateObject.GetField("_isLeaf")));
            return ret;
        }
        set {
            m_privateObject.SetField("_isLeaf", value);
        }
    }
    
    internal bool _isExpandedOnce {
        get {
            bool ret = ((bool)(m_privateObject.GetField("_isExpandedOnce")));
            return ret;
        }
        set {
            m_privateObject.SetField("_isExpandedOnce", value);
        }
    }
    
    internal bool _isExpanded {
        get {
            bool ret = ((bool)(m_privateObject.GetField("_isExpanded")));
            return ret;
        }
        set {
            m_privateObject.SetField("_isExpanded", value);
        }
    }
    
    internal global::Aga.Controls.Tree.TreeNodeAdv _parent {
        get {
            global::Aga.Controls.Tree.TreeNodeAdv ret = ((global::Aga.Controls.Tree.TreeNodeAdv)(m_privateObject.GetField("_parent")));
            return ret;
        }
        set {
            m_privateObject.SetField("_parent", value);
        }
    }
    
    internal global::Aga.Controls.Tree.TreeNodeAdv BottomNode {
        get {
            global::Aga.Controls.Tree.TreeNodeAdv ret = ((global::Aga.Controls.Tree.TreeNodeAdv)(m_privateObject.GetProperty("BottomNode")));
            return ret;
        }
    }
    
    internal global::Aga.Controls.Tree.TreeNodeAdv NextVisibleNode {
        get {
            global::Aga.Controls.Tree.TreeNodeAdv ret = ((global::Aga.Controls.Tree.TreeNodeAdv)(m_privateObject.GetProperty("NextVisibleNode")));
            return ret;
        }
    }
    
    internal object _tag {
        get {
            object ret = ((object)(m_privateObject.GetField("_tag")));
            return ret;
        }
        set {
            m_privateObject.SetField("_tag", value);
        }
    }
    
    internal System.Collections.ObjectModel.Collection<Aga.Controls.Tree.TreeNodeAdv> Nodes {
        get {
            System.Collections.ObjectModel.Collection<Aga.Controls.Tree.TreeNodeAdv> ret = ((System.Collections.ObjectModel.Collection<Aga.Controls.Tree.TreeNodeAdv>)(m_privateObject.GetProperty("Nodes")));
            return ret;
        }
    }
    
    internal System.Nullable<System.Drawing.Rectangle> _bounds {
        get {
            System.Nullable<System.Drawing.Rectangle> ret = ((System.Nullable<System.Drawing.Rectangle>)(m_privateObject.GetField("_bounds")));
            return ret;
        }
        set {
            m_privateObject.SetField("_bounds", value);
        }
    }
    
    internal System.Nullable<System.Drawing.Rectangle> Bounds {
        get {
            System.Nullable<System.Drawing.Rectangle> ret = ((System.Nullable<System.Drawing.Rectangle>)(m_privateObject.GetProperty("Bounds")));
            return ret;
        }
        set {
            m_privateObject.SetProperty("Bounds", value);
        }
    }
    
    internal static global::Aga.Controls.Tree.TreeNodeAdv CreatePrivate(global::Aga.Controls.Tree.TreeViewAdv tree, object tag) {
        object[] args = new object[] {
                tree,
                tag};
        Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject priv_obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(typeof(global::Aga.Controls.Tree.TreeNodeAdv), new System.Type[] {
                    typeof(global::Aga.Controls.Tree.TreeViewAdv),
                    typeof(object)}, args);
        return ((global::Aga.Controls.Tree.TreeNodeAdv)(priv_obj.Target));
    }
    
    internal void SetIsExpanded(bool value, bool ignoreChildren) {
        object[] args = new object[] {
                value,
                ignoreChildren};
        m_privateObject.Invoke("SetIsExpanded", new System.Type[] {
                    typeof(bool),
                    typeof(bool)}, args);
    }
    
    internal void SetIsExpandedRecursive(global::Aga.Controls.Tree.TreeNodeAdv root, bool value) {
        object[] args = new object[] {
                root,
                value};
        m_privateObject.Invoke("SetIsExpandedRecursive", new System.Type[] {
                    typeof(global::Aga.Controls.Tree.TreeNodeAdv),
                    typeof(bool)}, args);
    }
    
    internal void AssignIsExpanded(bool value) {
        object[] args = new object[] {
                value};
        m_privateObject.Invoke("AssignIsExpanded", new System.Type[] {
                    typeof(bool)}, args);
    }
    
    internal static global::Aga.Controls.Tree.TreeNodeAdv CreatePrivate(global::System.Runtime.Serialization.SerializationInfo info, global::System.Runtime.Serialization.StreamingContext context) {
        object[] args = new object[] {
                info,
                context};
        Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject priv_obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(typeof(global::Aga.Controls.Tree.TreeNodeAdv), new System.Type[] {
                    typeof(global::System.Runtime.Serialization.SerializationInfo),
                    typeof(global::System.Runtime.Serialization.StreamingContext)}, args);
        return ((global::Aga.Controls.Tree.TreeNodeAdv)(priv_obj.Target));
    }
}
}
