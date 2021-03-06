﻿#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UTTT.Ejemplo.Linq.Data.Entity;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Collections;
using UTTT.Ejemplo.Persona.Control;
using UTTT.Ejemplo.Persona.Control.Ctrl;
using EASendMail;


#endregion

namespace UTTT.Ejemplo.Persona
{
    public partial class PersonaManager : System.Web.UI.Page
    {
        #region Variables

        private SessionManager session = new SessionManager();
        private int idPersona = 0;
        private UTTT.Ejemplo.Linq.Data.Entity.Persona baseEntity;
        private DataContext dcGlobal = new DcGeneralDataContext();
        private int tipoAccion = 0;

        #endregion

        #region Eventos

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.Response.Buffer = true;
                this.session = (SessionManager)this.Session["SessionManager"];
                this.idPersona = this.session.Parametros["idPersona"] != null ?
                    int.Parse(this.session.Parametros["idPersona"].ToString()) : 0;
                if (this.idPersona == 0)
                {
                    this.baseEntity = new Linq.Data.Entity.Persona();
                    this.tipoAccion = 1;
                }
                else
                {
                    this.baseEntity = dcGlobal.GetTable<Linq.Data.Entity.Persona>().First(c => c.id == this.idPersona);
                    this.tipoAccion = 2;
                }

                if (!this.IsPostBack)
                {
                    if (this.session.Parametros["baseEntity"] == null)
                    {
                        this.session.Parametros.Add("baseEntity", this.baseEntity);
                    }
                    List<CatSexo> lista = dcGlobal.GetTable<CatSexo>().ToList();
                    CatSexo catTemp = new CatSexo();
                    catTemp.id = -1;
                    catTemp.strValor = "Seleccionar";
                    lista.Insert(0, catTemp);
                    this.ddlSexo.DataTextField = "strValor";
                    this.ddlSexo.DataValueField = "id";
                    this.ddlSexo.DataSource = lista;
                    this.ddlSexo.DataBind();

                    this.ddlSexo.SelectedIndexChanged += new EventHandler(ddlSexo_SelectedIndexChanged);
                    this.ddlSexo.AutoPostBack = true;
                    if (this.idPersona == 0)
                    {
                        this.lblAccion.Text = "Agregar";
                        DateTime tiempo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        this.dteCalendar.TodaysDate = tiempo;
                        this.dteCalendar.TodaysDate = tiempo;
                    }
                    else                                       

                    {
                        this.lblAccion.Text = "Editar";
                        ddlSexo.Enabled = false;
                        this.txtNombre.Text = this.baseEntity.strNombre;
                        this.txtAPaterno.Text = this.baseEntity.strAPaterno;
                        this.txtAMaterno.Text = this.baseEntity.strAMaterno;
                        this.txtClaveUnica.Text = this.baseEntity.strClaveUnica;
                        
                        DateTime? fechaNacimiento = this.baseEntity.dteFechaNacimiento;
                        if (fechaNacimiento != null)
                        {
                            this.dteCalendar.TodaysDate = (DateTime)fechaNacimiento;
                            this.dteCalendar.SelectedDate = (DateTime)fechaNacimiento;
                        }
                        this.txtCorreoElectronico.Text = this.baseEntity.strCorreoElectronico;
                        this.txtCodigoPostal.Text = this.baseEntity.intCodigoPostal.ToString();
                        this.txtRfc.Text = this.baseEntity.strRcf;


                        this.setItemEditar(ref this.ddlSexo, baseEntity.CatSexo.strValor);
                    }                
                }

            }
            catch (Exception _e)
            {
                this.showMessage("Ha Ocurrido un Problema al Cargar la Página");
                this.Response.Redirect("~/PersonaPrincipal.aspx", false);
            }

        }

        protected void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime fechaNacimiento1 = this.dteCalendar.SelectedDate.Date;
                DateTime fechaHoy = DateTime.Today;
                int edad = fechaHoy.Year - fechaNacimiento1.Year;
                if (fechaHoy < fechaNacimiento1.AddYears(edad)) edad--;

                if (edad < 18)
                {
                    this.showMessage("No puedes registrarte ya que eres menor de edad, <<Para Mayores de 18>>");
                    this.lblCalendario.Visible = true;

                }
                else
                {
                    if (!Page.IsValid)
                    {
                        return;
                    }
                    DataContext dcGuardar = new DcGeneralDataContext();
                    UTTT.Ejemplo.Linq.Data.Entity.Persona persona = new Linq.Data.Entity.Persona();
                    if (this.idPersona == 0)
                    {
                        persona.strClaveUnica = this.txtClaveUnica.Text.Trim();
                        persona.strNombre = this.txtNombre.Text.Trim();
                        persona.strAMaterno = this.txtAMaterno.Text.Trim();
                        persona.strAPaterno = this.txtAPaterno.Text.Trim();
                        persona.idCatSexo = int.Parse(this.ddlSexo.Text);
                        DateTime fechaNacimiento = this.dteCalendar.SelectedDate.Date;
                        persona.dteFechaNacimiento = fechaNacimiento;
                        persona.strCorreoElectronico = this.txtCorreoElectronico.Text.Trim();
                        persona.intCodigoPostal = int.Parse(this.txtCodigoPostal.Text);
                        persona.strRcf = this.txtRfc.Text.Trim();

                        String mensaje = String.Empty;
                        if (!this.validacion(persona, ref mensaje))

                        {
                            this.lblMensaje.Text = mensaje;
                            this.lblMensaje.Visible = true;
                            return;
                        }
                        if (!this.validaSql(ref mensaje))

                        {
                            this.lblMensaje.Text = mensaje;
                            this.lblMensaje.Visible = true;
                            return;
                        }
                        if (!this.validaHtml(ref mensaje))

                        {
                            this.lblMensaje.Text = mensaje;
                            this.lblMensaje.Visible = true;
                            return;
                        }


                        dcGuardar.GetTable<UTTT.Ejemplo.Linq.Data.Entity.Persona>().InsertOnSubmit(persona);
                        dcGuardar.SubmitChanges();
                        this.showMessage("El registro se Agrego con Exitó");
                        this.Response.Redirect("~/PersonaPrincipal.aspx", false);

                    }
                    if (this.idPersona > 0)
                    {
                        persona = dcGuardar.GetTable<UTTT.Ejemplo.Linq.Data.Entity.Persona>().First(c => c.id == idPersona);
                        persona.strClaveUnica = this.txtClaveUnica.Text.Trim();
                        persona.strNombre = this.txtNombre.Text.Trim();
                        persona.strAMaterno = this.txtAMaterno.Text.Trim();
                        persona.strAPaterno = this.txtAPaterno.Text.Trim();
                        persona.idCatSexo = int.Parse(this.ddlSexo.Text);
                        DateTime fechaNacimiento = this.dteCalendar.SelectedDate.Date;
                        persona.dteFechaNacimiento = fechaNacimiento;
                        persona.strCorreoElectronico = this.txtCorreoElectronico.Text.Trim();
                        persona.intCodigoPostal = int.Parse(this.txtCodigoPostal.Text);
                        persona.strRcf = this.txtRfc.Text.Trim();
                        dcGuardar.SubmitChanges();
                        this.showMessage("El registro se Edito con Exitó");
                        this.Response.Redirect("~/PersonaPrincipal.aspx", false);
                    }
                }
            }
            catch (Exception _e)
            {
                this.showMessageException(_e.Message);
                // Qué ha sucedido
                var mensaje = "Error message: " + _e.Message;
                // Información sobre la excepción interna
                if (_e.InnerException != null)
                {
                    mensaje = mensaje + " Inner exception: " + _e.InnerException.Message;
                }
                // Dónde ha sucedido
                mensaje = mensaje + " Stack trace: " + _e.StackTrace;
                this.Response.Redirect("~/ErrorPage.aspx", false);


                this.EnviarCorreo("carolinacanutoh@gmail.com", "ERROR", mensaje);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {              
                this.Response.Redirect("~/PersonaPrincipal.aspx", false);
            }
            catch (Exception _e)
            {
                this.showMessage("Ha ocurrido un Error Imprevisto");
            }
        }

        protected void ddlSexo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idSexo = int.Parse(this.ddlSexo.Text);
                Expression<Func<CatSexo, bool>> predicateSexo = c => c.id == idSexo;
                predicateSexo.Compile();
                List<CatSexo> lista = dcGlobal.GetTable<CatSexo>().Where(predicateSexo).ToList();
                CatSexo catTemp = new CatSexo();            
                this.ddlSexo.DataTextField = "strValor";
                this.ddlSexo.DataValueField = "id";
                this.ddlSexo.DataSource = lista;
                this.ddlSexo.DataBind();
            }
            catch (Exception)
            {
                this.showMessage("Ha ocurrido un Error Imprevisto");
            }
        }

        #endregion
        public void setItem(ref DropDownList _control, String _value)
        {
            foreach (ListItem item in _control.Items)
            {
                if (item.Value == _value)
                {
                    item.Selected = true;
                    break;
                }
            }
            _control.Items.FindByText(_value).Selected = true;
        }

        public void setItemEditar(ref DropDownList _control, String _value)
        {
            foreach (ListItem item in _control.Items)
            {
                if (item.Value == _value)
                {
                    item.Enabled = false;

                    break;
                }
            }
            _control.Items.FindByText(_value).Selected = true;
        }


        #region Metodos

        /// <param name="_persona"></param>
        /// <param name="_mensaje"></param>
        /// <returns></returns>
        /// 
        public bool validacion(UTTT.Ejemplo.Linq.Data.Entity.Persona _persona, ref String _mensaje)
        {
            
            if (_persona.idCatSexo == -1)
            {
                _mensaje = "Seleccione Su Generó Masculino/Femenino";
                return false;
            }
           
            int i = 0;
            if (int.TryParse(_persona.strClaveUnica, out i) == false)
            {
                _mensaje = "Sólo Números";
                return false;
            }
            

            if (int.Parse(_persona.strClaveUnica) < 100 || int.Parse(_persona.strClaveUnica) > 1000)
            {
                _mensaje = "Se Encuentra Fuera de Rango 100-1000";
                return false;
            }



            if (_persona.strNombre.Equals(String.Empty))
            {
                _mensaje = "El Campo Nombre se Encuentra Vacío";
                return false;
            }
            if (_persona.strNombre.Length > 50)
            {
                _mensaje = "Exedió el Número de Carácteres Permitidos";
                return false;
            }
            string nombre = _persona.strNombre.Trim();
            if (nombre.Length < 3)
            {
                _mensaje = "Escriba Más de Tres Carácteres";
                return false;
            }

            if (_persona.strAPaterno.Equals(String.Empty))
            {
                _mensaje = "El Campo Apellido Paterno se Encuentra Vacío";
                return false;
            }
            if (_persona.strAPaterno.Length > 50)
            {
                _mensaje = "Exedió el Número de Carácteres Permitidos";
                return false;
            }
            string strAPaterno = _persona.strAPaterno.Trim();
            if (strAPaterno.Length < 3)
            {
                _mensaje = "Escriba Más de Tres Carácteres";
                return false;
            }

            if (_persona.strAMaterno.Equals(String.Empty))
            {
                _mensaje = "El Campo Apellido Materno se Encuentra Vacío";
                return false;
            }
            if (_persona.strAMaterno.Length > 50)
            {
                _mensaje = "Exedió el Número de Carácteres Permitidos";
                return false;
            }
            string strAMaterno = _persona.strAMaterno.Trim();
            if (strAMaterno.Length < 3)
            {
                _mensaje = "Escriba Más de Tres Carácteres";
                return false;
            }

            if (_persona.strCorreoElectronico.Equals(String.Empty))
            {
                _mensaje = "El Campo Correo Eléctronico se Encuentra Vacío";
                return false;
            }
            if (_persona.strCorreoElectronico.Length > 50)
            {
                _mensaje = "Exedió el Número de Carácteres Permitidos";
                return false;
            }
            int j = 0;
            if (int.TryParse(_persona.intCodigoPostal.ToString(), out j) == false)
            {
                _mensaje = "El Campo Codigo Postal Solo Acepta Números";
                return false;
            }

            if (_persona.intCodigoPostal.Equals(String.Empty))
            {
                _mensaje = "El Campo Codigo Postal se Encuentra Vacío";
                return false;
            }

            
            if (_persona.strRcf.Equals(String.Empty))
            {
                _mensaje = "El Campo Rfc se Encuentra Vacío";
                return false;
            }
            if (_persona.strRcf.Length > 50)
            {
                _mensaje = "Exedió el Número de Carácteres Permitidos";
                return false;
            }


            return true;
        }

        private bool validaSql(ref String _mensaje)
        {
            CtrValidaInyeccion valida = new CtrValidaInyeccion();
            string mensajeFuncion = string.Empty;


            if (valida.SqlInyectionValida(this.txtNombre.Text.Trim(), ref mensajeFuncion, "Nombre", ref this.txtNombre))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.SqlInyectionValida(this.txtAPaterno.Text.Trim(), ref mensajeFuncion, "Apellido Paterno", ref this.txtAPaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.SqlInyectionValida(this.txtAMaterno.Text.Trim(), ref mensajeFuncion, "Apellido Materno", ref this.txtAMaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.SqlInyectionValida(this.txtCorreoElectronico.Text.Trim(), ref mensajeFuncion, "Correo Eléctronico", ref this.txtCorreoElectronico))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.SqlInyectionValida(this.txtCodigoPostal.Text.Trim(), ref mensajeFuncion, "Codigo Postal", ref this.txtCodigoPostal))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.SqlInyectionValida(this.txtRfc.Text.Trim(), ref mensajeFuncion, "Rfc", ref this.txtRfc))
            {
                _mensaje = mensajeFuncion;
                return false;
            }

            return true;
        }

        private bool validaHtml(ref String _mensaje)

        {
            CtrValidaInyeccion valida = new CtrValidaInyeccion();
            string mensajeFuncion = string.Empty;
            if (valida.htmlInyectionValida(this.txtClaveUnica.Text.Trim(), ref mensajeFuncion, "Clave Única", ref this.txtClaveUnica))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtNombre.Text.Trim(), ref mensajeFuncion, "Nombre", ref this.txtNombre))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAPaterno.Text.Trim(), ref mensajeFuncion, "Apellido Paterno", ref this.txtAPaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtAMaterno.Text.Trim(), ref mensajeFuncion, "Apellido Materno", ref this.txtAMaterno))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtCorreoElectronico.Text.Trim(), ref mensajeFuncion, "Correo Eléctronico", ref this.txtCorreoElectronico))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtCodigoPostal.Text.Trim(), ref mensajeFuncion, "Codigo Postal", ref this.txtCodigoPostal))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            if (valida.htmlInyectionValida(this.txtRfc.Text.Trim(), ref mensajeFuncion, "Rfc", ref this.txtRfc))
            {
                _mensaje = mensajeFuncion;
                return false;
            }
            return true;

        }

        public void EnviarCorreo(string correoDestino, string asunto, string mensajeCorreo)
        {
            string mensaje = "Error al enviar el Correo Eléctronico";

            try
            {
                SmtpMail objetoCorreo = new SmtpMail("TryIt");

                objetoCorreo.From = "carolinacanutoh@gmail.com";
                objetoCorreo.To = correoDestino;
                objetoCorreo.Subject = asunto;
                objetoCorreo.TextBody = mensajeCorreo;

                SmtpServer objetoServidor = new SmtpServer("smtp.gmail.com");

                objetoServidor.User = "carolinacanutoh@gmail.com";
                objetoServidor.Password = "canibalcorpsecaro3212";
                objetoServidor.Port = 587;
                objetoServidor.ConnectType = SmtpConnectType.ConnectSSLAuto;

                SmtpClient objetoCliente = new SmtpClient();
                objetoCliente.SendMail(objetoServidor, objetoCorreo);
                mensaje = "Correo Eléctronico Enviado con Exitó";


            }
            catch (Exception ex)
            {
                mensaje = "Error al Enviar Correo Eléctronico." + ex.Message;
            }
        }

        #endregion

        protected void dteCalendar_SelectionChanged(object sender, EventArgs e)
        {
            this.lblCalendario.Visible = false;
        }
    }
}