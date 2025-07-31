import React from 'react';
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
  IonButton,
  IonIcon,
  IonAvatar
} from '@ionic/react';
import { logOutOutline, personCircleOutline } from 'ionicons/icons';
import { useHistory } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { logout } from '../store/authSlice';

const Profile: React.FC = () => {
  const dispatch = useAppDispatch();
  const history = useHistory();
  const { user, isAuthenticated } = useAppSelector(state => state.auth);

  React.useEffect(() => {
    if (!isAuthenticated) {
      history.replace('/login');
    }
  }, [isAuthenticated, history]);

  const handleLogout = () => {
    dispatch(logout());
    history.replace('/login');
  };

  if (!user) return null;

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Mi Perfil</IonTitle>
        </IonToolbar>
      </IonHeader>
      
      <IonContent>
        <IonCard>
          <IonCardContent>
            <div style={{ textAlign: 'center', marginBottom: '1rem' }}>
              <IonAvatar style={{ width: '80px', height: '80px', margin: '0 auto 1rem' }}>
                <IonIcon icon={personCircleOutline} style={{ width: '100%', height: '100%' }} />
              </IonAvatar>
              <h2>{user.firstName} {user.lastName}</h2>
              <p style={{ color: 'gray' }}>{user.email}</p>
            </div>

            <IonItem>
              <IonLabel>
                <h3>Nombre</h3>
                <p>{user.firstName}</p>
              </IonLabel>
            </IonItem>

            <IonItem>
              <IonLabel>
                <h3>Apellido</h3>
                <p>{user.lastName}</p>
              </IonLabel>
            </IonItem>

            <IonItem>
              <IonLabel>
                <h3>Email</h3>
                <p>{user.email}</p>
              </IonLabel>
            </IonItem>

            <IonItem>
              <IonLabel>
                <h3>Rol</h3>
                <p>{user.role}</p>
              </IonLabel>
            </IonItem>

            <IonButton
              expand="block"
              color="danger"
              fill="outline"
              onClick={handleLogout}
              style={{ marginTop: '2rem' }}
            >
              <IonIcon icon={logOutOutline} slot="start" />
              Cerrar Sesión
            </IonButton>
          </IonCardContent>
        </IonCard>
      </IonContent>
    </IonPage>
  );
};

export default Profile;