from django.db import models
from django.forms import model_to_dict


class DbEntity(models.Model):
    typename = models.CharField(null=False, max_length=50)


class DbBoxCollision(models.Model):
    dbentity = models.OneToOneField(DbEntity, on_delete=models.CASCADE)
    x = models.IntegerField(null=True, default=0)
    y = models.IntegerField(null=True, default=0)
    width = models.IntegerField(null=True, default=0)
    height = models.IntegerField(null=True, default=0)


class DbDetails(models.Model):
    dbentity = models.OneToOneField(DbEntity, on_delete=models.CASCADE)
    name = models.CharField(max_length=30)


class DbInstance(models.Model):
    dbentity = models.OneToOneField(DbEntity, on_delete=models.CASCADE)
    x = models.IntegerField(null=True, default=0)
    y = models.IntegerField(null=True, default=0)
    # scene = models.ForeignKey


class DbUser(models.Model):
    dbinstance = models.OneToOneField(DbInstance, on_delete=models.CASCADE)
    username = models.CharField(max_length=30)
    password = models.CharField(max_length=200)


def construct_player_dbentity(username: str) -> DbEntity:
    entity = DbEntity(typename='Player')
    entity.save()

    details = DbDetails(dbentity=entity, name=username)
    details.save()

    collider = DbBoxCollision(dbentity=entity)
    collider.save()

    instance = DbInstance(dbentity=entity)
    instance.save()

    return entity


class Component:
    def __init__(self, entity_id):
        self.entity_id = entity_id
        self.table = None  # Db name. e.g. Transform.table = DbTransform

    def save(self):
        model = self.table.objects.get(dbentity_id=self.entity_id)
        modeldict = model_to_dict(model)
        for key, value in modeldict.items():
            if key == 'id' or key == 'dbentity_id' or key == 'dbentity':
                continue
            modeldict[key] = self.__getattribute__(key)

        modeldict.pop('dbentity')
        modeldict['dbentity_id'] = self.entity_id

        model = self.table(**modeldict)
        model.save()

    def load(self):
        modeldict = model_to_dict(self.table.objects.get(dbentity_id=self.entity_id))
        for key, value in modeldict.items():
            if key == 'id' or key == 'dbentity_id' or key == 'dbentity':
                continue
            self.__setattr__(key, value)


class Details(Component):
    def __init__(self, entity_id):
        super().__init__(entity_id)
        self.table = DbDetails

        self.name = ""

        self.load()


class BoxCollider(Component):
    def __init__(self, entity_id):
        super().__init__(entity_id)
        self.table = DbBoxCollision

        self.x = 0
        self.y = 0
        self.width = 0
        self.height = 0

        self.load()


class Entity:
    def __init__(self, pk: int):
        self.pk = pk
        self.typename = ""
        self.components = []

        self.load()

    def get_component(self, comptype):
        for comp in self.components:
            if isinstance(comp, comptype):
                return comp

    def add_components(self, *comptype):
        for ctype in comptype:
            for comp in self.components:
                if isinstance(comp, ctype):
                    raise Exception("Entity already has this component")
            self.components.append(ctype(self.pk))

    def save(self):
        for component in self.components:
            component.save()

    def load(self):
        dbentity = DbEntity.objects.get(pk=self.pk)
        self.typename = dbentity.typename

        if dbentity.typename == 'Player':
            self._load_player()
            pass

    def _load_player(self):
        # transform, render, boxcollider, details
        self.add_components(BoxCollider, Details)


class Instance:
    def __init__(self, pk):
        self.pk = pk
        self.x = 0
        self.y = 0
        self.dx = 0
        self.dy = 0

        dbi = DbInstance.objects.get(pk=pk)
        self.x = dbi.x
        self.y = dbi.y
        self.entity = Entity(dbi.dbentity_id)

    def save(self):
        dbi = DbInstance.objects.get(pk=self.pk)
        dbi.x = self.x
        dbi.y = self.y
        self.entity.save()
        dbi.save()

    def load(self):
        self.entity.load()
