using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonitorWeb
{
    public class FormAsync
    {
        private Form form;

        public FormAsync(Form form)
        {
            this.form = form;
        }

        delegate void ExecMethodAsycCallback(string metodo, params object[] liste);

        public void ExecMethodAsync(string metodo, params object[] liste)
        {
            if (form.InvokeRequired)
            {
                ExecMethodAsycCallback d = new ExecMethodAsycCallback(ExecMethod);
                form.Invoke(d, new object[] { metodo, liste });
            }
            else
            {
                ExecMethod(metodo, liste);
            }
        }

        public void ExecMethod(string metodo, params object[] liste)
        {
            Type thisType = form.GetType();
            MethodInfo theMethod = thisType.GetMethod(metodo);
            theMethod.Invoke(form, liste);
        }
    }
}
