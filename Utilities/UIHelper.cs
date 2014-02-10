// From multiple Internet sources e.g. http://www.hardcodet.net/2008/02/find-wpf-parent
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Johnson
{
    public static class UIHelpers
    {
        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        private static void FocusChildTextBox(ItemsControl container)
        {
            if (container.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                EventHandler eh = null;
                eh = new EventHandler(delegate
                {
                    if (container.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        FocusChildTextBox(container);

                        //remove the StatusChanged event handler since we just handled it (we only needed it once)
                        container.ItemContainerGenerator.StatusChanged -= eh;
                    }
                });
                container.ItemContainerGenerator.StatusChanged += eh;
            }
            else
            {
                TextBox tb = UIHelpers.FindVisualChild<TextBox>(container);
                if (tb != null && !tb.IsFocused)
                    tb.Focus();
            }
        }

        public static void SelectListBoxItem(ListBox lb, object itemToSelect)
        {
            ListBoxItem lbi = (ListBoxItem)lb.ItemContainerGenerator.ContainerFromItem(itemToSelect);

            if (lbi != null)
            {
                lbi.IsSelected = true;
                lbi.BringIntoView();

                TextBox tb = UIHelpers.FindVisualChild<TextBox>(lbi);
                if (tb != null && !tb.IsFocused)
                    tb.Focus();
            }
            else 
            {
                EventHandler eh = null;
                eh = new EventHandler(delegate
                {
                    if (lb.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        SelectListBoxItem(lb, itemToSelect);

                        //remove the StatusChanged event handler since we just handled it (we only needed it once)
                        lb.ItemContainerGenerator.StatusChanged -= eh;
                    }
                });
                lb.ItemContainerGenerator.StatusChanged += eh;
            }
        }

        public static bool ExpandAndSelectTreeViewItem(ItemsControl parentContainer, object itemToSelect)
        {
            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                if (item == itemToSelect && currentContainer != null)
                {
                    currentContainer.IsSelected = true;
                    currentContainer.BringIntoView();
                    FocusChildTextBox(currentContainer);

                    //the item was found
                    return true;
                }
            }

            foreach (object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    bool wasExpanded = currentContainer.IsExpanded;
                    currentContainer.IsExpanded = true;
                    if (currentContainer.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        EventHandler eh = null;
                        eh = new EventHandler(delegate
                        {
                            if (currentContainer.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                            {
                                if (ExpandAndSelectTreeViewItem(currentContainer, itemToSelect) == false)
                                {
                                    //The assumption is that code executing in this EventHandler is the result of the parent not
                                    //being expanded since the containers were not generated.
                                    //since the itemToSelect was not found in the children, collapse the parent since it was previously collapsed
                                    currentContainer.IsExpanded = false;
                                }

                                //remove the StatusChanged event handler since we just handled it (we only needed it once)
                                currentContainer.ItemContainerGenerator.StatusChanged -= eh;
                            }
                        });
                        currentContainer.ItemContainerGenerator.StatusChanged += eh;
                    }
                    else //otherwise the containers have been generated, so look for item to select in the children
                    {
                        if (ExpandAndSelectTreeViewItem(currentContainer, itemToSelect) == false)
                        {
                            //restore the current TreeViewItem’s expanded state
                            currentContainer.IsExpanded = wasExpanded;
                        }
                        else //otherwise the node was found and selected, so return true
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj)  where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }
    }
}
