MakeLuaPanelTemplate = {};

--ctor
function MakeLuaPanelTemplate:New(o, pGameObject)
    o = o or
        {
            gameObject = nil,
            transform = nil,
        };

    --define more members of 'o'


	setmetatable(o, self);
    self.__index = self;
    if (pGameObject) then
        o:InitPanel(pGameObject);
    end

	return o;
end


--initialize the panel with the passed gameObject
function MakeLuaPanelTemplate:InitPanel(pGameObject)

    self.gameObject = pGameObject;
    self.transform = pGameObject.transform;

    self:_FindComponent(pGameObject);

    --if other initialization instructions are necessary, write them here


end

function MakeLuaPanelTemplate:ReleasePanel()
    self.gameObject = nil;
    self.transform = nil;

    self:_ReleaseComponent();

    --if you want to handle other thing when releasing the panel, write them here


end
