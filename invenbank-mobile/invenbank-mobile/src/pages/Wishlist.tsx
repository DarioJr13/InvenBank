import React, { useEffect } from 'react';
import {
  IonContent,
  IonHeader,
  IonPage,
  IonTitle,
  IonToolbar,
  IonCard,
  IonCardContent,
  IonButton,
  IonIcon,
  IonSpinner,
  IonItem,
  IonLabel,
  IonBadge
} from '@ionic/react';
import { trashOutline, pricetagOutline } from 'ionicons/icons';
import { useHistory } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { getWishlistAsync, removeFromWishlistAsync } from '../store/wishlistSlice';

const Wishlist: React.FC = () => {
  const dispatch = useAppDispatch();
  const history = useHistory();
  const { items, loading } = useAppSelector(state => state.wishlist);
  const { isAuthenticated } = useAppSelector(state => state.auth);

  useEffect(() => {
    if (!isAuthenticated) {
      history.replace('/login');
      return;
    }
    dispatch(getWishlistAsync());
  }, [isAuthenticated, dispatch, history]);

  const removeFromWishlist = (productId: number) => {
    dispatch(removeFromWishlistAsync(productId));
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP'
    }).format(price);
  };

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Productos Favoritos</IonTitle>
        </IonToolbar>
      </IonHeader>
      
      <IonContent>
        {loading && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <IonSpinner />
            <p>Cargando favoritos...</p>
          </div>
        )}

        {!loading && items.length === 0 && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <p>No tienes productos favoritos aún</p>
            <IonButton routerLink="/products" fill="outline">
              Explorar Productos
            </IonButton>
          </div>
        )}

        {items.map((item) => (
          <IonCard key={item.id} button onClick={() => history.push(`/products/${item.productId}`)}>
            <IonCardContent>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div style={{ flex: 1 }}>
                  <h3 style={{ margin: '0 0 0.5rem 0' }}>{item.product.name}</h3>
                  <p style={{ color: 'gray', fontSize: '0.9rem', margin: '0 0 0.5rem 0' }}>
                    {item.product.description}
                  </p>
                  
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                    <IonIcon icon={pricetagOutline} />
                    <span style={{ fontWeight: 'bold', color: 'var(--ion-color-primary)' }}>
                      {formatPrice(item.product.minPrice)}
                      {item.product.minPrice !== item.product.maxPrice && (
                        <span style={{ fontWeight: 'normal', color: 'gray' }}>
                          {' - ' + formatPrice(item.product.maxPrice)}
                        </span>
                      )}
                    </span>
                  </div>

                  <IonBadge color={item.product.isAvailable ? 'success' : 'danger'}>
                    Stock: {item.product.totalStock}
                  </IonBadge>
                </div>

                <IonButton
                  fill="clear"
                  size="small"
                  color="danger"
                  onClick={(e) => {
                    e.stopPropagation();
                    removeFromWishlist(item.productId);
                  }}
                >
                  <IonIcon icon={trashOutline} />
                </IonButton>
              </div>
            </IonCardContent>
          </IonCard>
        ))}
      </IonContent>
    </IonPage>
  );
};

export default Wishlist;
