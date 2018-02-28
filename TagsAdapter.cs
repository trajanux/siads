using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Views;
using Android.Widget;

namespace CheckTag
{
    class TagsAdapter : BaseAdapter
    {
        List<Tag> tagList;
        Activity activity;

        public TagsAdapter(Activity act)
        {
            activity = act;
            tagList = new List<Tag>();
        }

        public override int Count
        {
            get { return tagList.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            // could wrap a Contact in a Java.Lang.Object to return it here if needed
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? activity.LayoutInflater.Inflate(Resource.Layout.TagListItem, parent, false);
            var tagdata = view.FindViewById<TextView>(Resource.Id.TagData);
            var checkdata = view.FindViewById<TextView>(Resource.Id.CheckData);
            var checkimage = view.FindViewById<ImageView>(Resource.Id.CheckImage);

            var tag = tagList[position];
            

            switch (tag.Status)
            {
                case Tag.TagState.Valid:
                    tagdata.Text = string.Format("EPC:{0}\nId:{1}", tag.Epc, tag.Id);
                    checkdata.Text = string.Format("Check:{2:X}  {3:X}\n", tag.Epc, tag.Id, tag.Check, tag.Memory);
                    checkimage.SetImageResource(Resource.Drawable.icon_check);
                    break;
                case Tag.TagState.Invalid:
                    tagdata.Text = string.Format("EPC:{0}\nId:{1}", tag.Epc, tag.Id);
                    checkdata.Text = string.Format("Check:{2:X}  {3:X}\n", tag.Epc, tag.Id, tag.Check, tag.Memory);
                    checkimage.SetImageResource(Resource.Drawable.icon_fail);
                    break;
                case Tag.TagState.Error:
                    tagdata.Text = tag.Epc;     // contains info about the error
                    checkdata.Text = "";
                    checkimage.SetImageResource(Resource.Drawable.icon_warning);
                    break;
                case Tag.TagState.Message:
                    tagdata.Text = tag.Epc;
                    checkdata.Text = "";
                    break;
            }
            return view;
        }

        public void AddNewTag(string m)
        {
            tagList.Add(new Tag (m));
            NotifyDataSetChanged();
        }

        public void AddMsgTag(string m)
        {
            Tag a = new Tag();
            a.Epc = m;
            tagList.Add(a);
            NotifyDataSetChanged();
        }

        public void ClearTags()
        {
            tagList.Clear();
            NotifyDataSetChanged();
        }
    }
}