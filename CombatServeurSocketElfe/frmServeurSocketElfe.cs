using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CombatServeurSocketElfe.Classes;

namespace CombatServeurSocketElfe
{
    public partial class frmServeurSocketElfe : Form
    {
        Random m_r;
        Nain m_nain;
        Elfe m_elfe;
        TcpListener m_ServerListener;
        Socket m_client;
        Thread m_thCombat;

        public frmServeurSocketElfe()
        {
            InitializeComponent();
            m_r = new Random();
            
            btnReset.Enabled = false;
            //Démarre un serveur de socket (TcpListener) fait
            m_ServerListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            m_ServerListener.Start();
            lstReception.Items.Add("Serveur démarré !");
            lstReception.Items.Add("PRESSER : << attendre un client >>");
            lstReception.Update();
            Control.CheckForIllegalCrossThreadCalls = false;
            Reset();
        }
        void Reset()
        {
            m_nain = new Nain(1, 0, 0);
            picNain.Image = m_nain.Avatar;
            AfficheStatNain();

            m_elfe = new Elfe(m_r.Next(10, 20), m_r.Next(2, 6), m_r.Next(2, 6));
            picElfe.Image = m_elfe.Avatar;
            AfficheStatElfe();
 
            lstReception.Items.Clear();
        }

        void AfficheStatNain()
        {
            lblVieNain.Text = "Vie: " + m_nain.Vie.ToString();
            lblForceNain.Text = "Force: " + m_nain.Force.ToString();
            lblArmeNain.Text = "Arme: " + m_nain.Arme;
            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        void AfficheStatElfe()
        {
            lblVieElfe.Text = "Vie: " + m_elfe.Vie.ToString();
            lblForceElfe.Text = "Force: " + m_elfe.Force.ToString();
            lblSortElfe.Text = "Sort: " + m_elfe.Sort.ToString();
            this.Update(); // pour s'assurer de l'affichage via le thread
        }
        private void btnReset_Click(object sender, EventArgs e)
        {

            Reset();
        }     

        private void btnAttente_Click(object sender, EventArgs e)
        {
            // Combat par un thread
            /* Déclaration d'un client */
            Socket client = null;
            string reponseServeur = "aucune";
            string receptionClient = "rien";
            int nbOctetreception;
            byte[] tByteReception = new byte[20];
            ASCIIEncoding textByte = new ASCIIEncoding();
            byte[] tByteEnvoie;
            try
            {
                //initialisation d'un client (bloquant) 
                client = m_ServerListener.AcceptSocket(); //(bloquant, en attente)
                lstReception.Items.Add("Client branché!");
                lstReception.Update();
                Thread.Sleep(500); // donne du temps
                nbOctetreception = client.Receive(tByteReception);
                receptionClient = Encoding.ASCII.GetString(tByteReception);
                

                lstReception.Update();
                tByteEnvoie = textByte.GetBytes(reponseServeur);
                client.Send(tByteEnvoie);
                // Fermeture du client
                client.Close();
            }
            catch (Exception ex)
            {
                lstReception.Items.Add("Server not ready! CATCH exception");
                lstReception.Items.Add(ex.Message);
                lstReception.Update();
            }
            Thread.Sleep(500);
            lstReception.Items.Add("PRESS : << attendre un client >>");
            lstReception.Update();
            btnFermer.Enabled = true;

        }
        public void Combat() 
        {
            // déclarations de variables locales 
            string reponseServer = "aucune";
            string receptionClient = "rien";
            int nbOctetReception;
            int noArme = 0, vie = 0, force = 0;
            string arme;
            byte[] tByteReception = new byte[50];
            ASCIIEncoding textByte = new ASCIIEncoding();
            byte[] tByteEnvoie;

            try
            {
                // tous le code de traitement
                while (m_nain.Vie == 0 || m_elfe.Vie == 0)
                {
                    m_client = m_ServerListener.AcceptSocket();
                    lstReception.Items.Add("Client branché!");
                    lstReception.Update();
                    Thread.Sleep(500);

                    //recoit les données cliente (nain)
                    nbOctetReception = m_client.Receive(tByteReception);
                    receptionClient = Encoding.ASCII.GetString(tByteReception);

                    lstReception.Items.Add("du client: " + receptionClient);
                    lstReception.Update();
                    //split sur le ; pour récuperer les données du nain 
                    string[] substring = receptionClient.Split(';');
                    arme = substring[2];

                    AfficheStatNain();

                    //execute frapper
                    MessageBox.Show("Serveur: Frapper l'elfe ");
                    m_nain.Frapper(m_elfe);

                    //affiche les données de l'elfe membre
                    AfficheStatElfe();

                    //execute lancer sort
                    MessageBox.Show("Serveur: Lancer un sort au nain");
                    m_elfe.LancerSort(m_nain);
                    AfficheStatNain();
                    AfficheStatElfe();

                    reponseServer = "vieNain;forceNain;armeNain;vieElfe;forceElfe;sortElfe;";

                    lstReception.Items.Add(reponseServer);
                    lstReception.Update();

                    tByteEnvoie = textByte.GetBytes(reponseServer);

                    m_client.Send(tByteEnvoie);
                    Thread.Sleep(500);
                    //ferme le socket
                    m_client.Close();
                }
                //vérifier le gagnant
                if (m_nain.Vie > m_elfe.Vie)
                {
                    MessageBox.Show("Le nain gagne!!");
                }
                else
                {
                    MessageBox.Show("L'elfe gagne!!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            // il faut avoir un objet elfe et un objet nain instanciés
            //m_elfe.Vie = 0;
            //m_nain.Vie = 0;
            try
            {
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void frmServeurSocketElfe_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnFermer_Click(sender,e);
            try
            {
                // il faut avoir un objet TCPListener existant
                m_ServerListener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception: " + ex.Message);
            }
        }
    }
}
