import React, { useState, useEffect } from 'react';
import {
  IonContent,
  IonHeader,
  IonPage,
  IonTitle,
  IonToolbar,
  IonCard,
  IonCardContent,
  IonItem,
  IonLabel,
  IonInput,
  IonButton,
  IonAlert,
  IonSpinner,
  IonIcon
} from '@ionic/react';
import { logIn } from 'ionicons/icons';
import { useHistory } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { loginAsync, clearError } from '../store/authSlice';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showAlert, setShowAlert] = useState(false);
  
  const dispatch = useAppDispatch();
  const history = useHistory();
  const { loading, error, isAuthenticated } = useAppSelector(state => state.auth);

  useEffect(() => {
    if (isAuthenticated) {
      history.replace('/products');
    }
  }, [isAuthenticated, history]);

  useEffect(() => {
    if (error) {
      setShowAlert(true);
    }
  }, [error]);

  const handleLogin = async () => {
    if (!email || !password) {
      return;
    }

    dispatch(loginAsync({ email, password }));
  };

  const handleKeyPress = (e: any) => {
    if (e.key === 'Enter') {
      handleLogin();
    }
  };

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>InvenBank</IonTitle>
        </IonToolbar>
      </IonHeader>
      
      <IonContent className="ion-padding">
        <div style={{ display: 'flex', justifyContent: 'center', paddingTop: '20%' }}>
          <IonCard style={{ width: '100%', maxWidth: '400px' }}>
            <IonCardContent>
              <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
                <IonIcon icon={logIn} style={{ fontSize: '4rem', color: 'var(--ion-color-primary)' }} />
                <h2>Iniciar Sesión</h2>
                <p>Ingresa tus credenciales para continuar</p>
              </div>

              <IonItem>
                <IonLabel position="stacked">Email</IonLabel>
                <IonInput
                  type="email"
                  value={email}
                  onIonInput={(e) => setEmail(e.detail.value!)}
                  onKeyPress={handleKeyPress}
                  placeholder="tu@email.com"
                />
              </IonItem>

              <IonItem>
                <IonLabel position="stacked">Contraseña</IonLabel>
                <IonInput
                  type="password"
                  value={password}
                  onIonInput={(e) => setPassword(e.detail.value!)}
                  onKeyPress={handleKeyPress}
                  placeholder="••••••••"
                />
              </IonItem>

              <IonButton
                expand="block"
                onClick={handleLogin}
                disabled={loading || !email || !password}
                style={{ marginTop: '1rem' }}
              >
                {loading ? <IonSpinner name="crescent" /> : 'Iniciar Sesión'}
              </IonButton>

              <div style={{ marginTop: '1rem', textAlign: 'center', fontSize: '0.8rem', color: 'gray' }}>
                <p>Usuarios de prueba:</p>
                <p>admin@invenbank.com / Admin123!</p>
                <p>cliente@invenbank.com / Cliente123!</p>
              </div>
            </IonCardContent>
          </IonCard>
        </div>

        <IonAlert
          isOpen={showAlert}
          onDidDismiss={() => {
            setShowAlert(false);
            dispatch(clearError());
          }}
          header="Error de Login"
          message={error || 'Credenciales incorrectas'}
          buttons={['OK']}
        />
      </IonContent>
    </IonPage>
  );
};

export default Login;