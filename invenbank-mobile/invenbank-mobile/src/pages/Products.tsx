import React, { useEffect, useState } from 'react';
import {
  IonContent,
  IonHeader,
  IonPage,
  IonTitle,
  IonToolbar,
  IonSearchbar,
  IonCard,
  IonCardContent,
  IonItem,
  IonLabel,
  IonSpinner,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonButton,
  IonIcon,
  IonBadge,
  IonRefresher,
  IonRefresherContent,
  IonToast
} from '@ionic/react';
import { heart, heartOutline, pricetagOutline } from 'ionicons/icons';
import { useHistory } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/useAppDispatch';
import { searchProductsAsync, setSearchTerm } from '../store/productsSlice';
import { addToWishlistAsync, removeFromWishlistAsync, getWishlistAsync } from '../store/wishlistSlice';

const Products: React.FC = () => {
  const [searchText, setSearchText] = useState('');
  const [toastMessage, setToastMessage] = useState('');
  const [showToast, setShowToast] = useState(false);
  const [toastColor, setToastColor] = useState<'success' | 'danger'>('success');

  const dispatch = useAppDispatch();
  const history = useHistory();

  const { products, loading, searchTerm, pagination } = useAppSelector(state => state.products);
  const { items: wishlistItems } = useAppSelector(state => state.wishlist);
  const { isAuthenticated } = useAppSelector(state => state.auth);

  useEffect(() => {
    if (!isAuthenticated) {
      history.replace('/login');
      return;
    }
    dispatch(searchProductsAsync({ pageNumber: 1, pageSize: 20 }));
    dispatch(getWishlistAsync());
  }, [isAuthenticated, dispatch, history]);

  useEffect(() => {
    const handleFocus = () => {
      if (isAuthenticated) {
        dispatch(getWishlistAsync());
      }
    };
    window.addEventListener('focus', handleFocus);
    return () => window.removeEventListener('focus', handleFocus);
  }, [dispatch, isAuthenticated]);

  const handleSearch = (e: CustomEvent) => {
    const term = e.detail.value!;
    setSearchText(term);
    dispatch(setSearchTerm(term));
    setTimeout(() => {
      dispatch(searchProductsAsync({ searchTerm: term, pageNumber: 1, pageSize: 20 }));
    }, 500);
  };

  const loadMore = async (e: CustomEvent) => {
    if (pagination.hasNextPage) {
      await dispatch(searchProductsAsync({
        searchTerm,
        pageNumber: pagination.pageNumber + 1,
        pageSize: 20
      }));
    }
    (e.target as HTMLIonInfiniteScrollElement).complete();
  };

  const handleRefresh = async (e: CustomEvent) => {
    await dispatch(searchProductsAsync({ searchTerm, pageNumber: 1, pageSize: 20 }));
    (e.target as HTMLIonRefresherElement).complete();
  };

  const showNotification = (message: string, color: 'success' | 'danger' = 'success') => {
    setToastMessage(message);
    setToastColor(color);
    setShowToast(true);
  };

  const toggleWishlist = async (productId: number) => {
    const isInWishlist = wishlistItems.some(item => item.ProductId === productId);
    try {
      if (isInWishlist) {
        await dispatch(removeFromWishlistAsync(productId)).unwrap();
        showNotification('Producto removido de favoritos');
      } else {
        await dispatch(addToWishlistAsync(productId)).unwrap();
        showNotification('Producto agregado a favoritos');
      }
      dispatch(getWishlistAsync());
    } catch (error) {
      console.error('❌ Error toggleWishlist:', error);
      showNotification('Error al actualizar favoritos', 'danger');
    }
  };

  const formatPrice = (price: number) => {
    if (!price || isNaN(price)) return '$0.00';
    return new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP' }).format(price);
  };

  return (
    <IonPage>
      <IonHeader>
        <IonToolbar>
          <IonTitle>Productos</IonTitle>
        </IonToolbar>
      </IonHeader>
      <IonContent>
        <IonRefresher slot="fixed" onIonRefresh={handleRefresh}>
          <IonRefresherContent />
        </IonRefresher>

        <IonSearchbar
          value={searchText}
          onIonInput={handleSearch}
          placeholder="Buscar productos..."
          debounce={300}
        />

        {loading && !products.length && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <IonSpinner />
            <p>Cargando productos...</p>
          </div>
        )}

        {products.map((product) => {
          const isInWishlist = wishlistItems.some(item => item.ProductId === product.Id);
          return (
            <IonCard key={product.Id} button onClick={() => history.push(`/products/${product.Id}`)}>
              <IonCardContent>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ flex: 1 }}>
                    <h3 style={{ margin: '0 0 0.5rem 0' }}>{product.Name}</h3>
                    <p style={{ color: 'gray', fontSize: '0.9rem', margin: '0 0 0.5rem 0' }}>{product.Description}</p>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                      <IonIcon icon={pricetagOutline} />
                      <span style={{ fontWeight: 'bold', color: 'var(--ion-color-primary)' }}>
                        {formatPrice(product.MinPrice)}
                        {product.MinPrice !== product.MaxPrice && (
                          <span style={{ fontWeight: 'normal', color: 'gray' }}>
                            {' - ' + formatPrice(product.MaxPrice)}
                          </span>
                        )}
                      </span>
                    </div>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      <IonBadge color={product.TotalStock > 0 ? 'success' : 'danger'}>
                        Stock: {product.TotalStock}
                      </IonBadge>
                      <IonBadge color="medium">{product.Category}</IonBadge>
                    </div>
                  </div>
                  <IonButton
                    fill="clear"
                    size="small"
                    onClick={(e) => {
                      e.stopPropagation();
                      toggleWishlist(product.Id);
                    }}
                  >
                    <IonIcon
                      icon={isInWishlist ? heart : heartOutline}
                      color={isInWishlist ? 'danger' : 'medium'}
                    />
                  </IonButton>
                </div>
              </IonCardContent>
            </IonCard>
          );
        })}

        {!loading && products.length === 0 && (
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <p>No se encontraron productos</p>
          </div>
        )}

        <IonInfiniteScroll onIonInfinite={loadMore}>
          <IonInfiniteScrollContent loadingText="Cargando más productos..." />
        </IonInfiniteScroll>

        <IonToast
          isOpen={showToast}
          onDidDismiss={() => setShowToast(false)}
          message={toastMessage}
          duration={2000}
          color={toastColor}
          position="top"
        />
      </IonContent>
    </IonPage>
  );
};

export default Products;
