using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogNub.Logger
{
    public class SmallSerializer
    {
        public static string Wrap(string name, string value)
        {
            return String.Format("{{\"{0}\":{1}}}", name, value);
        }
        public static string SerializeObject(object o, bool readable = false)
        {
            StringBuilder builder = new StringBuilder();
            SerializeObject(builder,
                                  o, readable);

            return builder.ToString();
        }

        public static void SerializeObject(StringBuilder builder, object o, bool readable = false)
        {

            builder.Append("{");
            if (readable)
                builder.Append("\n");
            Type t = o.GetType();
            var fields = t.GetProperties();
            for (int index = 0; index < fields.Length; index++)
            {
                var o2 = fields[index];
                builder.Append(string.Format("\"{0}\":",
                             o2.Name));


                var value = o2.GetValue(o);
                if (value == null)
                {
                    builder.Append("null");
                }
                else
                if (value.GetType().IsEnum)
                {
                    builder.Append(((int)value));

                }
                else
                if (value.IsNumericType())
                {
                    builder.Append(value);
                    if (readable)
                        builder.Append("\n");
                }
                else
                {

                    if (o2.PropertyType.IsValueType || value is string)
                    {
                        builder.Append(string.Format("\"{0}\"", value).Replace("\\", "_").Replace(Environment.NewLine, "\n"));

                    }
                    else
                    {


                        if (value is IEnumerable)
                        {
                            var lst = value as IEnumerable;
                            builder.Append("[");
                            bool remove = false;
                            foreach (var i in lst)
                            {
                                if (readable)
                                    builder.Append("\n");
                                SerializeObject(builder,
                                                i, readable);
                                builder.Append(",");
                                remove = true;
                            }
                            if (remove)
                                builder.Remove(builder.Length - 1,
                                               1);
                            builder.Append("]");
                        }
                        else
                        {
                            SerializeObject(builder, value, readable);
                        }
                    }
                }
                if (index < fields.Length - 1)
                {
                    builder.Append(",");
                }
                if (readable)
                    builder.Append("\n");

            }

            builder.Append("}");

        }
    }
}
