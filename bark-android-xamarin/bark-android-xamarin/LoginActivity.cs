
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel.DataAnnotations;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace barkandroidxamarin {
    
	[Activity(Label = "bark-android-xamarin", MainLauncher = true, Icon = "@mipmap/icon")]
	public class LoginActivity : Activity {
        private const String TAG = "LoginActivity";
        private const int REQUEST_NEW_ACCOUNT = 0;

		private EditText emailField;
		private EditText passwordField;
		private Button loginButton;
		private TextView createAccountLabel;
		private String userEmail, userPassword;
        private BackendServer server = new BackendServer();

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
			// Create your application here
            SetContentView(Resource.Layout.activity_login);
            initialize();
		}

		/**
		 * Initializing values, listeners, and other stuff.
		 **/
		public void initialize() {
			emailField = FindViewById<EditText>(Resource.Id.loginEmail); //(EditText)findViewById(R.id.loginEmail);
			passwordField = FindViewById<EditText>(Resource.Id.loginPassword); //(EditText)findViewById(R.id.loginPassword);
			loginButton = FindViewById<Button>(Resource.Id.loginButton); //(Button)findViewById(R.id.loginButton);
			createAccountLabel = FindViewById<TextView>(Resource.Id.createAccountLabel); //(TextView)findViewById(R.id.createAccountLabel);
			userEmail = "";
			userPassword = "";
			//server = new backendServer(selfLoginActivity);

			//Setting up clickListeners
			loginButton.Click += (sender, e) => {
			    login();
			};

			// Start the createAccount activity when the label is clicked.
			createAccountLabel.Click += (sender, e) => {
				var myIntent = new Intent(this, typeof(CreateAccountActivity));
				StartActivityForResult(myIntent, REQUEST_NEW_ACCOUNT);
			};

			//Checks if the user has logged in before and if that is the case then the user is
			// automatically sent to the home screen.
			if(server.isUserLoggedInBefore()) {
			    var myIntent = new Intent(this, typeof(MainActivity));
			    StartActivity(myIntent);
			    Finish();
			}
        }

		/**
		 * This function handles login validations.
		 * */
        public void login() {
			Log.Info(TAG, "Login");
            if (!validate()) {
                Toast.MakeText(this, "Login failed", ToastLength.Long).Show();
				return;
            }
            loginButton.Enabled = false;
            ProgressDialog progressDialog = new ProgressDialog(this);
			progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
			progressDialog.Indeterminate = true;
            progressDialog.SetMessage("Authenticating...");
			progressDialog.SetCancelable(false);
			progressDialog.Show();
			new Thread(new ThreadStart(() => {
				Thread.Sleep(3 * 1000);
				this.RunOnUiThread(() => {
					loginButton.Enabled = true;//.setEnabled(true);
					progressDialog.Dismiss();
					if (onLoginSuccess()) {
						Toast.MakeText(this, "Login Succeed", ToastLength.Long).Show();
						var myIntent = new Intent(this, typeof(MainActivity));
						StartActivity(myIntent);
						Finish();
					}
				});
			})).Start();
        }

		/**
	     * Sets up the email field after the user successfully sets up a new account.
	     * */
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);
			if (resultCode == Result.Ok && requestCode == REQUEST_NEW_ACCOUNT) {
				emailField.Text = data.GetStringExtra("userEmail");
				passwordField.RequestFocus();
			}
		}

		/**
		 * Checks the password and email are in the database.
		 * */
		public Boolean onLoginSuccess() {
			return server.isUserOnServer(userEmail, userPassword);
		}

		/**
		 * Checks the email and password fields are not empty and makes sure the text entered in the
		 * email field is an email address. Else, the fields show an error for invalidation.
		 * */
		public Boolean validate() {
            Boolean valid = true;
            userEmail = emailField.Text;
			userPassword = passwordField.Text;

            if (string.IsNullOrEmpty(userEmail)) {
				emailField.SetError("Enter a valid email address", null);
				valid = false;
			}
			else {
				emailField.SetError("", null);
			}

            if (string.IsNullOrEmpty(userPassword)) {
                passwordField.SetError("Please enter your password", null);
				valid = false;
			}
			else {
				passwordField.SetError("", null);
			}

			return valid;
		}
    }
}
