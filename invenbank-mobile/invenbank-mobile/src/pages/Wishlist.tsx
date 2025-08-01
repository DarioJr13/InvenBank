import React, { useEffect } from 'react';
import {
  IonPage, IonHeader, IonToolbar, IonTitle,
  IonContent, IonCard, IonCardContent, IonItem,
  IonLabel, IonBadge, IonSpinner, IonButton, IonIcon
} from '@ionic/react';
import { heart, pricetagOutline } from 'ionicons/icons';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { getWishlistAsync } from '../store/wishlistSlice';
import { useHistory } from 'react-router-dom';

const Wishlist: React.FC = () => {
  const dispatch = useAppDispatch();
  const history = useHistory();

  const { items: wishlistItems } = useAppSelector(state => state.wishlist);
  const { isAuthenticated } = useAppSelector(state => state.auth);

  useEffect(() => {
    if (!isAuthenticated) {
      history.replace('/login');
      return;
    }
    dispatch(getWishlistAsync());
  }, [dispatch, isAuthenticated, history]);

  const formatPrice = (price: number) => {
    if (!price || isNaN(price)) return '$0.00';
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
    }).format(price);
  };

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Favoritos</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent>
        {wishlistItems.length === 0 && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <IonSpinner />
            <p>No tienes productos en tu lista de deseos.</p>
          </div>
        )}

        {wishlistItems.map((item) => (
          <IonCard key={item.ProductId} button onClick={() => history.push(`/products/${item.ProductId}`)}>
            <IonCardContent>
              <IonItem>
                <IonLabel>
                  <h3>{item.Name}</h3>
                  <p>{item.Description}</p>
                  <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
                    <IonBadge color="primary">{formatPrice(item.MinPrice)}</IonBadge>
                    <IonBadge color="medium">{item.Category}</IonBadge>
                  </div>
                </IonLabel>
                <IonIcon icon={heart} color="danger" />
              </IonItem>
            </IonCardContent>
          </IonCard>
        ))}
      </IonContent>
    </IonPage>
  );
};

export default Wishlist;
